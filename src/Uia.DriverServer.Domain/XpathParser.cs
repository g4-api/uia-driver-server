using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UIAutomationClient;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Provides methods to parse XPath strings and convert them to UI Automation conditions.
    /// </summary>
    public static class XpathParser
    {
        // The UI Automation instance
        private static readonly CUIAutomation8 s_automation = new();

        // The string comparer for case-insensitive comparisons
        private static readonly StringComparer s_comparer = StringComparer.OrdinalIgnoreCase;

        // Maps a control-type name to the (field name, control-type id) of its UIA_ControlTypeIds constant. Built as a
        // static table rather than by reflection so the keys are curated instead of derived from the field name.
        private static readonly Dictionary<string, (string Field, int Id)> s_controlTypeMapping = NewControlTypeMapping();

        // Maps a property name to the (field name, property id) of its UIA_PropertyIds constant. Pattern properties are
        // registered under several aliases (concatenated, dotted, and Pattern-qualified) so an Inspector-style name such
        // as "Annotation.DateTime" or "AnnotationPattern.DateTime" resolves as well as the legacy "AnnotationDateTime".
        private static readonly Dictionary<string, (string Field, int Id)> s_propertyIdMapping = NewPropertyIdMapping();

        /// <summary>
        /// Converts an XPath string to a UI Automation condition.
        /// </summary>
        /// <param name="xpath">The XPath string to convert.</param>
        /// <returns>An <see cref="IUIAutomationCondition"/> representing the condition tree constructed from the XPath string.</returns>
        public static IUIAutomationCondition ConvertToCondition(string xpath)
        {
            // Format the XPath string into segments
            var segments = FormatXpath(xpath);

            // Create a new condition tree from the formatted segments
            return NewConditionTree(segments);
        }

        // Formats the given XPath string by extracting control types, logical operators, parentheses, and conditions.
        private static List<string> FormatXpath(string input)
        {
            // Define the regex pattern to match control types, logical operators, parentheses, and conditions. The
            // condition property name allows dots so dotted pattern-property aliases (for example @Annotation.DateTime)
            // tokenize as a single predicate instead of splitting at the dot.
            const string pattern = @"(?<controlType>/{0,2}\w+)|(?<logical>\band\b|\bor\b|\bnot\b)|(?<parentheses>[\(\)])|(?<condition>@[\w\-.]+='[^']*')";

            // Find matches in the xpath string based on the defined pattern
            var matches = Regex.Matches(
                input,
                pattern,
                options: RegexOptions.None,
                matchTimeout: TimeSpan.FromSeconds(30)
            );

            // Convert the matches to a list of strings, trim whitespace, and filter out empty tokens
            return [.. matches.Cast<Match>()
                .Select(match => match.Value.Trim(' ', '/'))
                .Where(token => !string.IsNullOrEmpty(token))];
        }

        // Creates a new UI Automation condition tree based on the specified segments.
        private static IUIAutomationCondition NewConditionTree(List<string> segments)
        {
            // Create stacks for conditions
            var conditionStack = new Stack<IUIAutomationCondition>();

            // Create a stack for operators
            var operatorStack = new Stack<string>();

            // Initialize the control type condition
            IUIAutomationCondition controlTypeCondition = null;

            // Create a regular expression pattern for logical operators (and, or, not) in a case-insensitive manner
            var logicalOperatorPattern = new Regex(
                pattern: "(?is)^(and|or|not)$",
                options: RegexOptions.None,
                matchTimeout: TimeSpan.FromSeconds(30)
            );

            // Process each segment in the list of segments from the XPath expression string
            foreach (var segment in segments)
            {
                // Check if the segment is a control type
                if (s_controlTypeMapping.ContainsKey(segment))
                {
                    // Create a control type condition
                    controlTypeCondition = NewControlTypeCondition(segment);
                }
                // Check if the segment is a logical operator (and, or, not)
                else if (logicalOperatorPattern.IsMatch(input: segment))
                {
                    operatorStack.Push(segment);
                }
                // Check if the segment is an opening parenthesis
                else if (segment.Equals("(", StringComparison.OrdinalIgnoreCase))
                {
                    operatorStack.Push(segment);
                }
                // Check if the segment is a closing parenthesis
                else if (segment.Equals(")", StringComparison.OrdinalIgnoreCase))
                {
                    while (operatorStack.Peek() != "(")
                    {
                        // Create a new condition based on the operator and push it to the condition stack
                        var condition = NewOperatorCondition(operatorStack.Pop(), conditionStack);

                        // Push the condition to the condition stack
                        conditionStack.Push(condition);
                    }

                    // Pop the opening parenthesis from the operator stack
                    operatorStack.Pop();
                }
                // Check if the segment is a property condition
                else if (segment.StartsWith('@'))
                {
                    // Create a property condition and push it to the condition stack
                    conditionStack.Push(NewPropertyCondition(segment));
                }
            }

            // Process remaining operators in the operator stack
            while (operatorStack.Count > 0)
            {
                // Create a new condition based on the operator
                var condition = NewOperatorCondition(operatorStack.Pop(), conditionStack);

                // Push the condition to the condition stack
                conditionStack.Push(condition);
            }

            // Combine the control type condition with the condition stack if necessary
            if (controlTypeCondition != null && conditionStack.Count > 0)
            {
                // Createa combined condition based on the control type condition and the top condition in the stack
                var combinedCondition = s_automation.CreateAndCondition(controlTypeCondition, conditionStack.Pop());

                // Push the combined condition to the condition stack
                conditionStack.Push(combinedCondition);
            }

            // Return the final condition from the condition stack or the control type condition if the stack is empty
            return conditionStack.Count > 0 ? conditionStack.Pop() : controlTypeCondition;
        }

        // Creates a new UI Automation condition for the specified control type.
        private static IUIAutomationCondition NewControlTypeCondition(string controlType)
        {
            // Check if the control type is mapped to a control type ID
            var isId = s_controlTypeMapping.TryGetValue(key: controlType, out var entry);

            // Throw an exception if the control type is not supported
            if (!isId)
            {
                throw new NotSupportedException($"Unsupported control type: {controlType}");
            }

            // Create and return the property condition for the control type
            return s_automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, entry.Id);
        }

        // Creates a new UI Automation condition based on a logical operator and a stack of conditions.
        private static IUIAutomationCondition NewOperatorCondition(string logicalOperator, Stack<IUIAutomationCondition> conditionStack)
        {
            // Check for the logical operator and create the corresponding condition
            if (logicalOperator.Equals("and", StringComparison.OrdinalIgnoreCase))
            {
                // Pop two conditions from the stack and create an AND condition
                var right = conditionStack.Pop();
                var left = conditionStack.Pop();

                // Create and return the AND condition
                return s_automation.CreateAndCondition(left, right);
            }
            else if (logicalOperator.Equals("or", StringComparison.OrdinalIgnoreCase))
            {
                // Pop two conditions from the stack and create an OR condition
                var right = conditionStack.Pop();
                var left = conditionStack.Pop();

                // Create and return the OR condition
                return s_automation.CreateOrCondition(left, right);
            }
            else if (logicalOperator.Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                // Pop one condition from the stack and create a NOT condition
                var condition = conditionStack.Pop();

                // Create and return the NOT condition
                return s_automation.CreateNotCondition(condition);
            }

            // Throw an exception if the logical operator is not supported
            throw new NotSupportedException($"Unsupported operator: {logicalOperator}");
        }

        // Creates a new UI Automation condition for properties based on the specified segment.
        private static IUIAutomationCondition NewPropertyCondition(string segment)
        {
            // Remove the leading character and split the segment into property name and value
            var parts = segment[1..].Split('=');
            var propertyName = parts[0].Trim();
            var value = parts[1].Trim('\'', '\"');

            // Determine if the condition should match a substring
            var conditionFlag = propertyName.StartsWith("partial", StringComparison.OrdinalIgnoreCase)
                ? PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
                : PropertyConditionFlags.PropertyConditionFlags_None;

            // Remove "partial" from the property name if present
            propertyName = Regex.Replace(
                input: propertyName,
                pattern: "(?is)^partial",
                replacement: string.Empty, options: RegexOptions.None,
                matchTimeout: TimeSpan.FromSeconds(30)
            );

            // Check if the property name is mapped to a property ID
            var isId = s_propertyIdMapping.TryGetValue(key: propertyName, out var entry);

            // Throw an exception if the property is not supported
            if (!isId)
            {
                throw new NotSupportedException($"Unsupported property: {propertyName}");
            }

            // Create and return the property condition with the specified flags
            return s_automation.CreatePropertyConditionEx(entry.Id, value, conditionFlag);
        }

        // Builds the control-type name to (field, id) table from the UIA_ControlTypeIds constants.
        private static Dictionary<string, (string Field, int Id)> NewControlTypeMapping()
        {
            var map = new Dictionary<string, (string Field, int Id)>(s_comparer);

            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ButtonControlTypeId), UIA_ControlTypeIds.UIA_ButtonControlTypeId, "Button");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_CalendarControlTypeId), UIA_ControlTypeIds.UIA_CalendarControlTypeId, "Calendar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_CheckBoxControlTypeId), UIA_ControlTypeIds.UIA_CheckBoxControlTypeId, "CheckBox");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ComboBoxControlTypeId), UIA_ControlTypeIds.UIA_ComboBoxControlTypeId, "ComboBox");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_EditControlTypeId), UIA_ControlTypeIds.UIA_EditControlTypeId, "Edit");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_HyperlinkControlTypeId), UIA_ControlTypeIds.UIA_HyperlinkControlTypeId, "Hyperlink");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ImageControlTypeId), UIA_ControlTypeIds.UIA_ImageControlTypeId, "Image");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ListItemControlTypeId), UIA_ControlTypeIds.UIA_ListItemControlTypeId, "ListItem");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ListControlTypeId), UIA_ControlTypeIds.UIA_ListControlTypeId, "List");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_MenuControlTypeId), UIA_ControlTypeIds.UIA_MenuControlTypeId, "Menu");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_MenuBarControlTypeId), UIA_ControlTypeIds.UIA_MenuBarControlTypeId, "MenuBar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_MenuItemControlTypeId), UIA_ControlTypeIds.UIA_MenuItemControlTypeId, "MenuItem");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ProgressBarControlTypeId), UIA_ControlTypeIds.UIA_ProgressBarControlTypeId, "ProgressBar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_RadioButtonControlTypeId), UIA_ControlTypeIds.UIA_RadioButtonControlTypeId, "RadioButton");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ScrollBarControlTypeId), UIA_ControlTypeIds.UIA_ScrollBarControlTypeId, "ScrollBar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_SliderControlTypeId), UIA_ControlTypeIds.UIA_SliderControlTypeId, "Slider");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_SpinnerControlTypeId), UIA_ControlTypeIds.UIA_SpinnerControlTypeId, "Spinner");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_StatusBarControlTypeId), UIA_ControlTypeIds.UIA_StatusBarControlTypeId, "StatusBar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TabControlTypeId), UIA_ControlTypeIds.UIA_TabControlTypeId, "Tab");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TabItemControlTypeId), UIA_ControlTypeIds.UIA_TabItemControlTypeId, "TabItem");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TextControlTypeId), UIA_ControlTypeIds.UIA_TextControlTypeId, "Text");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ToolBarControlTypeId), UIA_ControlTypeIds.UIA_ToolBarControlTypeId, "ToolBar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ToolTipControlTypeId), UIA_ControlTypeIds.UIA_ToolTipControlTypeId, "ToolTip");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TreeControlTypeId), UIA_ControlTypeIds.UIA_TreeControlTypeId, "Tree");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TreeItemControlTypeId), UIA_ControlTypeIds.UIA_TreeItemControlTypeId, "TreeItem");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_CustomControlTypeId), UIA_ControlTypeIds.UIA_CustomControlTypeId, "Custom");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_GroupControlTypeId), UIA_ControlTypeIds.UIA_GroupControlTypeId, "Group");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_ThumbControlTypeId), UIA_ControlTypeIds.UIA_ThumbControlTypeId, "Thumb");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_DataGridControlTypeId), UIA_ControlTypeIds.UIA_DataGridControlTypeId, "DataGrid");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_DataItemControlTypeId), UIA_ControlTypeIds.UIA_DataItemControlTypeId, "DataItem");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_DocumentControlTypeId), UIA_ControlTypeIds.UIA_DocumentControlTypeId, "Document");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_SplitButtonControlTypeId), UIA_ControlTypeIds.UIA_SplitButtonControlTypeId, "SplitButton");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_WindowControlTypeId), UIA_ControlTypeIds.UIA_WindowControlTypeId, "Window");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_PaneControlTypeId), UIA_ControlTypeIds.UIA_PaneControlTypeId, "Pane");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_HeaderControlTypeId), UIA_ControlTypeIds.UIA_HeaderControlTypeId, "Header");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_HeaderItemControlTypeId), UIA_ControlTypeIds.UIA_HeaderItemControlTypeId, "HeaderItem");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TableControlTypeId), UIA_ControlTypeIds.UIA_TableControlTypeId, "Table");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_TitleBarControlTypeId), UIA_ControlTypeIds.UIA_TitleBarControlTypeId, "TitleBar");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_SeparatorControlTypeId), UIA_ControlTypeIds.UIA_SeparatorControlTypeId, "Separator");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_SemanticZoomControlTypeId), UIA_ControlTypeIds.UIA_SemanticZoomControlTypeId, "SemanticZoom");
            AddControlType(map, nameof(UIA_ControlTypeIds.UIA_AppBarControlTypeId), UIA_ControlTypeIds.UIA_AppBarControlTypeId, "AppBar");

            return map;
        }

        // Builds the property name to (field, id) table from the UIA_PropertyIds constants. Element properties get a
        // single key; pattern properties get concatenated, dotted, and Pattern-qualified aliases.
        private static Dictionary<string, (string Field, int Id)> NewPropertyIdMapping()
        {
            var map = new Dictionary<string, (string Field, int Id)>(s_comparer);

            // Element (non-pattern) properties.
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_RuntimeIdPropertyId), UIA_PropertyIds.UIA_RuntimeIdPropertyId, "RuntimeId");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_BoundingRectanglePropertyId), UIA_PropertyIds.UIA_BoundingRectanglePropertyId, "BoundingRectangle");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ProcessIdPropertyId), UIA_PropertyIds.UIA_ProcessIdPropertyId, "ProcessId");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ControlTypePropertyId), UIA_PropertyIds.UIA_ControlTypePropertyId, "ControlType");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_LocalizedControlTypePropertyId), UIA_PropertyIds.UIA_LocalizedControlTypePropertyId, "LocalizedControlType");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_NamePropertyId), UIA_PropertyIds.UIA_NamePropertyId, "Name");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AcceleratorKeyPropertyId), UIA_PropertyIds.UIA_AcceleratorKeyPropertyId, "AcceleratorKey");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AccessKeyPropertyId), UIA_PropertyIds.UIA_AccessKeyPropertyId, "AccessKey");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_HasKeyboardFocusPropertyId), UIA_PropertyIds.UIA_HasKeyboardFocusPropertyId, "HasKeyboardFocus");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsKeyboardFocusablePropertyId), UIA_PropertyIds.UIA_IsKeyboardFocusablePropertyId, "IsKeyboardFocusable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsEnabledPropertyId), UIA_PropertyIds.UIA_IsEnabledPropertyId, "IsEnabled");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AutomationIdPropertyId), UIA_PropertyIds.UIA_AutomationIdPropertyId, "AutomationId");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ClassNamePropertyId), UIA_PropertyIds.UIA_ClassNamePropertyId, "ClassName");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_HelpTextPropertyId), UIA_PropertyIds.UIA_HelpTextPropertyId, "HelpText");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ClickablePointPropertyId), UIA_PropertyIds.UIA_ClickablePointPropertyId, "ClickablePoint");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_CulturePropertyId), UIA_PropertyIds.UIA_CulturePropertyId, "Culture");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsControlElementPropertyId), UIA_PropertyIds.UIA_IsControlElementPropertyId, "IsControlElement");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsContentElementPropertyId), UIA_PropertyIds.UIA_IsContentElementPropertyId, "IsContentElement");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_LabeledByPropertyId), UIA_PropertyIds.UIA_LabeledByPropertyId, "LabeledBy");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsPasswordPropertyId), UIA_PropertyIds.UIA_IsPasswordPropertyId, "IsPassword");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_NativeWindowHandlePropertyId), UIA_PropertyIds.UIA_NativeWindowHandlePropertyId, "NativeWindowHandle");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ItemTypePropertyId), UIA_PropertyIds.UIA_ItemTypePropertyId, "ItemType");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsOffscreenPropertyId), UIA_PropertyIds.UIA_IsOffscreenPropertyId, "IsOffscreen");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_OrientationPropertyId), UIA_PropertyIds.UIA_OrientationPropertyId, "Orientation");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_FrameworkIdPropertyId), UIA_PropertyIds.UIA_FrameworkIdPropertyId, "FrameworkId");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsRequiredForFormPropertyId), UIA_PropertyIds.UIA_IsRequiredForFormPropertyId, "IsRequiredForForm");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ItemStatusPropertyId), UIA_PropertyIds.UIA_ItemStatusPropertyId, "ItemStatus");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsDockPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsDockPatternAvailablePropertyId, "IsDockPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsExpandCollapsePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsExpandCollapsePatternAvailablePropertyId, "IsExpandCollapsePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsGridItemPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsGridItemPatternAvailablePropertyId, "IsGridItemPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsGridPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsGridPatternAvailablePropertyId, "IsGridPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsInvokePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsInvokePatternAvailablePropertyId, "IsInvokePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsMultipleViewPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsMultipleViewPatternAvailablePropertyId, "IsMultipleViewPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsRangeValuePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsRangeValuePatternAvailablePropertyId, "IsRangeValuePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsScrollPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsScrollPatternAvailablePropertyId, "IsScrollPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsScrollItemPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsScrollItemPatternAvailablePropertyId, "IsScrollItemPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsSelectionItemPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsSelectionItemPatternAvailablePropertyId, "IsSelectionItemPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsSelectionPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsSelectionPatternAvailablePropertyId, "IsSelectionPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTablePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTablePatternAvailablePropertyId, "IsTablePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTableItemPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTableItemPatternAvailablePropertyId, "IsTableItemPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTextPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTextPatternAvailablePropertyId, "IsTextPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTogglePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTogglePatternAvailablePropertyId, "IsTogglePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTransformPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTransformPatternAvailablePropertyId, "IsTransformPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsValuePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsValuePatternAvailablePropertyId, "IsValuePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsWindowPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsWindowPatternAvailablePropertyId, "IsWindowPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsLegacyIAccessiblePatternAvailablePropertyId), UIA_PropertyIds.UIA_IsLegacyIAccessiblePatternAvailablePropertyId, "IsLegacyIAccessiblePatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AriaRolePropertyId), UIA_PropertyIds.UIA_AriaRolePropertyId, "AriaRole");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AriaPropertiesPropertyId), UIA_PropertyIds.UIA_AriaPropertiesPropertyId, "AriaProperties");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsDataValidForFormPropertyId), UIA_PropertyIds.UIA_IsDataValidForFormPropertyId, "IsDataValidForForm");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ControllerForPropertyId), UIA_PropertyIds.UIA_ControllerForPropertyId, "ControllerFor");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_DescribedByPropertyId), UIA_PropertyIds.UIA_DescribedByPropertyId, "DescribedBy");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_FlowsToPropertyId), UIA_PropertyIds.UIA_FlowsToPropertyId, "FlowsTo");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_ProviderDescriptionPropertyId), UIA_PropertyIds.UIA_ProviderDescriptionPropertyId, "ProviderDescription");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsItemContainerPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsItemContainerPatternAvailablePropertyId, "IsItemContainerPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsVirtualizedItemPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsVirtualizedItemPatternAvailablePropertyId, "IsVirtualizedItemPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsSynchronizedInputPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsSynchronizedInputPatternAvailablePropertyId, "IsSynchronizedInputPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_OptimizeForVisualContentPropertyId), UIA_PropertyIds.UIA_OptimizeForVisualContentPropertyId, "OptimizeForVisualContent");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsObjectModelPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsObjectModelPatternAvailablePropertyId, "IsObjectModelPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsAnnotationPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsAnnotationPatternAvailablePropertyId, "IsAnnotationPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTextPattern2AvailablePropertyId), UIA_PropertyIds.UIA_IsTextPattern2AvailablePropertyId, "IsTextPattern2Available");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsStylesPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsStylesPatternAvailablePropertyId, "IsStylesPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsSpreadsheetPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsSpreadsheetPatternAvailablePropertyId, "IsSpreadsheetPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsSpreadsheetItemPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsSpreadsheetItemPatternAvailablePropertyId, "IsSpreadsheetItemPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTransformPattern2AvailablePropertyId), UIA_PropertyIds.UIA_IsTransformPattern2AvailablePropertyId, "IsTransformPattern2Available");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_LiveSettingPropertyId), UIA_PropertyIds.UIA_LiveSettingPropertyId, "LiveSetting");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTextChildPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTextChildPatternAvailablePropertyId, "IsTextChildPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsDragPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsDragPatternAvailablePropertyId, "IsDragPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsDropTargetPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsDropTargetPatternAvailablePropertyId, "IsDropTargetPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_FlowsFromPropertyId), UIA_PropertyIds.UIA_FlowsFromPropertyId, "FlowsFrom");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsTextEditPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsTextEditPatternAvailablePropertyId, "IsTextEditPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsPeripheralPropertyId), UIA_PropertyIds.UIA_IsPeripheralPropertyId, "IsPeripheral");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsCustomNavigationPatternAvailablePropertyId), UIA_PropertyIds.UIA_IsCustomNavigationPatternAvailablePropertyId, "IsCustomNavigationPatternAvailable");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_PositionInSetPropertyId), UIA_PropertyIds.UIA_PositionInSetPropertyId, "PositionInSet");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_SizeOfSetPropertyId), UIA_PropertyIds.UIA_SizeOfSetPropertyId, "SizeOfSet");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_LevelPropertyId), UIA_PropertyIds.UIA_LevelPropertyId, "Level");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationTypesPropertyId), UIA_PropertyIds.UIA_AnnotationTypesPropertyId, "AnnotationTypes");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationObjectsPropertyId), UIA_PropertyIds.UIA_AnnotationObjectsPropertyId, "AnnotationObjects");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_LandmarkTypePropertyId), UIA_PropertyIds.UIA_LandmarkTypePropertyId, "LandmarkType");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_LocalizedLandmarkTypePropertyId), UIA_PropertyIds.UIA_LocalizedLandmarkTypePropertyId, "LocalizedLandmarkType");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_FullDescriptionPropertyId), UIA_PropertyIds.UIA_FullDescriptionPropertyId, "FullDescription");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_FillColorPropertyId), UIA_PropertyIds.UIA_FillColorPropertyId, "FillColor");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_OutlineColorPropertyId), UIA_PropertyIds.UIA_OutlineColorPropertyId, "OutlineColor");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_FillTypePropertyId), UIA_PropertyIds.UIA_FillTypePropertyId, "FillType");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_VisualEffectsPropertyId), UIA_PropertyIds.UIA_VisualEffectsPropertyId, "VisualEffects");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_OutlineThicknessPropertyId), UIA_PropertyIds.UIA_OutlineThicknessPropertyId, "OutlineThickness");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_CenterPointPropertyId), UIA_PropertyIds.UIA_CenterPointPropertyId, "CenterPoint");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_RotationPropertyId), UIA_PropertyIds.UIA_RotationPropertyId, "Rotation");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_SizePropertyId), UIA_PropertyIds.UIA_SizePropertyId, "Size");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsSelectionPattern2AvailablePropertyId), UIA_PropertyIds.UIA_IsSelectionPattern2AvailablePropertyId, "IsSelectionPattern2Available");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_HeadingLevelPropertyId), UIA_PropertyIds.UIA_HeadingLevelPropertyId, "HeadingLevel");
            AddElementProperty(map, nameof(UIA_PropertyIds.UIA_IsDialogPropertyId), UIA_PropertyIds.UIA_IsDialogPropertyId, "IsDialog");

            // Value pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ValueValuePropertyId), UIA_PropertyIds.UIA_ValueValuePropertyId, "Value", "Value");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ValueIsReadOnlyPropertyId), UIA_PropertyIds.UIA_ValueIsReadOnlyPropertyId, "Value", "IsReadOnly");

            // RangeValue pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_RangeValueValuePropertyId), UIA_PropertyIds.UIA_RangeValueValuePropertyId, "RangeValue", "Value");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_RangeValueIsReadOnlyPropertyId), UIA_PropertyIds.UIA_RangeValueIsReadOnlyPropertyId, "RangeValue", "IsReadOnly");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_RangeValueMinimumPropertyId), UIA_PropertyIds.UIA_RangeValueMinimumPropertyId, "RangeValue", "Minimum");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_RangeValueMaximumPropertyId), UIA_PropertyIds.UIA_RangeValueMaximumPropertyId, "RangeValue", "Maximum");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_RangeValueLargeChangePropertyId), UIA_PropertyIds.UIA_RangeValueLargeChangePropertyId, "RangeValue", "LargeChange");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_RangeValueSmallChangePropertyId), UIA_PropertyIds.UIA_RangeValueSmallChangePropertyId, "RangeValue", "SmallChange");

            // Scroll pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ScrollHorizontalScrollPercentPropertyId), UIA_PropertyIds.UIA_ScrollHorizontalScrollPercentPropertyId, "Scroll", "HorizontalScrollPercent");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ScrollHorizontalViewSizePropertyId), UIA_PropertyIds.UIA_ScrollHorizontalViewSizePropertyId, "Scroll", "HorizontalViewSize");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ScrollVerticalScrollPercentPropertyId), UIA_PropertyIds.UIA_ScrollVerticalScrollPercentPropertyId, "Scroll", "VerticalScrollPercent");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ScrollVerticalViewSizePropertyId), UIA_PropertyIds.UIA_ScrollVerticalViewSizePropertyId, "Scroll", "VerticalViewSize");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ScrollHorizontallyScrollablePropertyId), UIA_PropertyIds.UIA_ScrollHorizontallyScrollablePropertyId, "Scroll", "HorizontallyScrollable");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ScrollVerticallyScrollablePropertyId), UIA_PropertyIds.UIA_ScrollVerticallyScrollablePropertyId, "Scroll", "VerticallyScrollable");

            // Selection pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SelectionSelectionPropertyId), UIA_PropertyIds.UIA_SelectionSelectionPropertyId, "Selection", "Selection");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SelectionCanSelectMultiplePropertyId), UIA_PropertyIds.UIA_SelectionCanSelectMultiplePropertyId, "Selection", "CanSelectMultiple");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SelectionIsSelectionRequiredPropertyId), UIA_PropertyIds.UIA_SelectionIsSelectionRequiredPropertyId, "Selection", "IsSelectionRequired");

            // Grid pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridRowCountPropertyId), UIA_PropertyIds.UIA_GridRowCountPropertyId, "Grid", "RowCount");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridColumnCountPropertyId), UIA_PropertyIds.UIA_GridColumnCountPropertyId, "Grid", "ColumnCount");

            // GridItem pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridItemRowPropertyId), UIA_PropertyIds.UIA_GridItemRowPropertyId, "GridItem", "Row");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridItemColumnPropertyId), UIA_PropertyIds.UIA_GridItemColumnPropertyId, "GridItem", "Column");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridItemRowSpanPropertyId), UIA_PropertyIds.UIA_GridItemRowSpanPropertyId, "GridItem", "RowSpan");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridItemColumnSpanPropertyId), UIA_PropertyIds.UIA_GridItemColumnSpanPropertyId, "GridItem", "ColumnSpan");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_GridItemContainingGridPropertyId), UIA_PropertyIds.UIA_GridItemContainingGridPropertyId, "GridItem", "ContainingGrid");

            // Dock pattern property.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DockDockPositionPropertyId), UIA_PropertyIds.UIA_DockDockPositionPropertyId, "Dock", "DockPosition");

            // ExpandCollapse pattern property.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ExpandCollapseExpandCollapseStatePropertyId), UIA_PropertyIds.UIA_ExpandCollapseExpandCollapseStatePropertyId, "ExpandCollapse", "ExpandCollapseState");

            // MultipleView pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_MultipleViewCurrentViewPropertyId), UIA_PropertyIds.UIA_MultipleViewCurrentViewPropertyId, "MultipleView", "CurrentView");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_MultipleViewSupportedViewsPropertyId), UIA_PropertyIds.UIA_MultipleViewSupportedViewsPropertyId, "MultipleView", "SupportedViews");

            // Window pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_WindowCanMaximizePropertyId), UIA_PropertyIds.UIA_WindowCanMaximizePropertyId, "Window", "CanMaximize");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_WindowCanMinimizePropertyId), UIA_PropertyIds.UIA_WindowCanMinimizePropertyId, "Window", "CanMinimize");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_WindowWindowVisualStatePropertyId), UIA_PropertyIds.UIA_WindowWindowVisualStatePropertyId, "Window", "WindowVisualState");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_WindowWindowInteractionStatePropertyId), UIA_PropertyIds.UIA_WindowWindowInteractionStatePropertyId, "Window", "WindowInteractionState");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_WindowIsModalPropertyId), UIA_PropertyIds.UIA_WindowIsModalPropertyId, "Window", "IsModal");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_WindowIsTopmostPropertyId), UIA_PropertyIds.UIA_WindowIsTopmostPropertyId, "Window", "IsTopmost");

            // SelectionItem pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SelectionItemIsSelectedPropertyId), UIA_PropertyIds.UIA_SelectionItemIsSelectedPropertyId, "SelectionItem", "IsSelected");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SelectionItemSelectionContainerPropertyId), UIA_PropertyIds.UIA_SelectionItemSelectionContainerPropertyId, "SelectionItem", "SelectionContainer");

            // Table pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TableRowHeadersPropertyId), UIA_PropertyIds.UIA_TableRowHeadersPropertyId, "Table", "RowHeaders");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TableColumnHeadersPropertyId), UIA_PropertyIds.UIA_TableColumnHeadersPropertyId, "Table", "ColumnHeaders");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TableRowOrColumnMajorPropertyId), UIA_PropertyIds.UIA_TableRowOrColumnMajorPropertyId, "Table", "RowOrColumnMajor");

            // TableItem pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TableItemRowHeaderItemsPropertyId), UIA_PropertyIds.UIA_TableItemRowHeaderItemsPropertyId, "TableItem", "RowHeaderItems");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TableItemColumnHeaderItemsPropertyId), UIA_PropertyIds.UIA_TableItemColumnHeaderItemsPropertyId, "TableItem", "ColumnHeaderItems");

            // Toggle pattern property.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_ToggleToggleStatePropertyId), UIA_PropertyIds.UIA_ToggleToggleStatePropertyId, "Toggle", "ToggleState");

            // Transform pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TransformCanMovePropertyId), UIA_PropertyIds.UIA_TransformCanMovePropertyId, "Transform", "CanMove");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TransformCanResizePropertyId), UIA_PropertyIds.UIA_TransformCanResizePropertyId, "Transform", "CanResize");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_TransformCanRotatePropertyId), UIA_PropertyIds.UIA_TransformCanRotatePropertyId, "Transform", "CanRotate");

            // LegacyIAccessible pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleChildIdPropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleChildIdPropertyId, "LegacyIAccessible", "ChildId");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleNamePropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleNamePropertyId, "LegacyIAccessible", "Name");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleValuePropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleValuePropertyId, "LegacyIAccessible", "Value");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleDescriptionPropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleDescriptionPropertyId, "LegacyIAccessible", "Description");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleRolePropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleRolePropertyId, "LegacyIAccessible", "Role");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleStatePropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleStatePropertyId, "LegacyIAccessible", "State");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleHelpPropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleHelpPropertyId, "LegacyIAccessible", "Help");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleKeyboardShortcutPropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleKeyboardShortcutPropertyId, "LegacyIAccessible", "KeyboardShortcut");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleSelectionPropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleSelectionPropertyId, "LegacyIAccessible", "Selection");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_LegacyIAccessibleDefaultActionPropertyId), UIA_PropertyIds.UIA_LegacyIAccessibleDefaultActionPropertyId, "LegacyIAccessible", "DefaultAction");

            // Annotation pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationAnnotationTypeIdPropertyId), UIA_PropertyIds.UIA_AnnotationAnnotationTypeIdPropertyId, "Annotation", "AnnotationTypeId");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationAnnotationTypeNamePropertyId), UIA_PropertyIds.UIA_AnnotationAnnotationTypeNamePropertyId, "Annotation", "AnnotationTypeName");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationAuthorPropertyId), UIA_PropertyIds.UIA_AnnotationAuthorPropertyId, "Annotation", "Author");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationDateTimePropertyId), UIA_PropertyIds.UIA_AnnotationDateTimePropertyId, "Annotation", "DateTime");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_AnnotationTargetPropertyId), UIA_PropertyIds.UIA_AnnotationTargetPropertyId, "Annotation", "Target");

            // Styles pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesStyleIdPropertyId), UIA_PropertyIds.UIA_StylesStyleIdPropertyId, "Styles", "StyleId");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesStyleNamePropertyId), UIA_PropertyIds.UIA_StylesStyleNamePropertyId, "Styles", "StyleName");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesFillColorPropertyId), UIA_PropertyIds.UIA_StylesFillColorPropertyId, "Styles", "FillColor");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesFillPatternStylePropertyId), UIA_PropertyIds.UIA_StylesFillPatternStylePropertyId, "Styles", "FillPatternStyle");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesShapePropertyId), UIA_PropertyIds.UIA_StylesShapePropertyId, "Styles", "Shape");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesFillPatternColorPropertyId), UIA_PropertyIds.UIA_StylesFillPatternColorPropertyId, "Styles", "FillPatternColor");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_StylesExtendedPropertiesPropertyId), UIA_PropertyIds.UIA_StylesExtendedPropertiesPropertyId, "Styles", "ExtendedProperties");

            // SpreadsheetItem pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SpreadsheetItemFormulaPropertyId), UIA_PropertyIds.UIA_SpreadsheetItemFormulaPropertyId, "SpreadsheetItem", "Formula");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SpreadsheetItemAnnotationObjectsPropertyId), UIA_PropertyIds.UIA_SpreadsheetItemAnnotationObjectsPropertyId, "SpreadsheetItem", "AnnotationObjects");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_SpreadsheetItemAnnotationTypesPropertyId), UIA_PropertyIds.UIA_SpreadsheetItemAnnotationTypesPropertyId, "SpreadsheetItem", "AnnotationTypes");

            // Transform2 pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Transform2CanZoomPropertyId), UIA_PropertyIds.UIA_Transform2CanZoomPropertyId, "Transform2", "CanZoom");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Transform2ZoomLevelPropertyId), UIA_PropertyIds.UIA_Transform2ZoomLevelPropertyId, "Transform2", "ZoomLevel");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Transform2ZoomMinimumPropertyId), UIA_PropertyIds.UIA_Transform2ZoomMinimumPropertyId, "Transform2", "ZoomMinimum");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Transform2ZoomMaximumPropertyId), UIA_PropertyIds.UIA_Transform2ZoomMaximumPropertyId, "Transform2", "ZoomMaximum");

            // Drag pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DragIsGrabbedPropertyId), UIA_PropertyIds.UIA_DragIsGrabbedPropertyId, "Drag", "IsGrabbed");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DragDropEffectPropertyId), UIA_PropertyIds.UIA_DragDropEffectPropertyId, "Drag", "DropEffect");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DragDropEffectsPropertyId), UIA_PropertyIds.UIA_DragDropEffectsPropertyId, "Drag", "DropEffects");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DragGrabbedItemsPropertyId), UIA_PropertyIds.UIA_DragGrabbedItemsPropertyId, "Drag", "GrabbedItems");

            // DropTarget pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DropTargetDropTargetEffectPropertyId), UIA_PropertyIds.UIA_DropTargetDropTargetEffectPropertyId, "DropTarget", "DropTargetEffect");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_DropTargetDropTargetEffectsPropertyId), UIA_PropertyIds.UIA_DropTargetDropTargetEffectsPropertyId, "DropTarget", "DropTargetEffects");

            // Selection2 pattern properties.
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Selection2FirstSelectedItemPropertyId), UIA_PropertyIds.UIA_Selection2FirstSelectedItemPropertyId, "Selection2", "FirstSelectedItem");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Selection2LastSelectedItemPropertyId), UIA_PropertyIds.UIA_Selection2LastSelectedItemPropertyId, "Selection2", "LastSelectedItem");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Selection2CurrentSelectedItemPropertyId), UIA_PropertyIds.UIA_Selection2CurrentSelectedItemPropertyId, "Selection2", "CurrentSelectedItem");
            AddPatternProperty(map, nameof(UIA_PropertyIds.UIA_Selection2ItemCountPropertyId), UIA_PropertyIds.UIA_Selection2ItemCountPropertyId, "Selection2", "ItemCount");

            return map;
        }

        // Adds a control type under its single name key.
        private static void AddControlType(Dictionary<string, (string Field, int Id)> map, string field, int id, string name)
        {
            map[name] = (field, id);
        }

        // Adds an element (non-pattern) property under its single name key.
        private static void AddElementProperty(Dictionary<string, (string Field, int Id)> map, string field, int id, string name)
        {
            map[name] = (field, id);
        }

        // Adds a pattern property under three alias keys so legacy, dotted, and Pattern-qualified spellings all resolve:
        // "PatternProperty" (legacy concatenated), "Pattern.Property" (dotted), and "PatternPattern.Property".
        private static void AddPatternProperty(Dictionary<string, (string Field, int Id)> map, string field, int id, string pattern, string property)
        {
            var entry = (field, id);

            map[$"{pattern}{property}"] = entry;
            map[$"{pattern}.{property}"] = entry;
            map[$"{pattern}Pattern.{property}"] = entry;
        }
    }
}
