using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public class RenamerFieldSet : RadzenComponent
{
    private bool collapsed;

    private
#nullable disable
            string contentStyle = "";

    private string summaryContentStyle = "display: none";

    /// <inheritdoc />
    protected override string GetComponentCssClass()
    {
        return !this.AllowCollapse ? "rz-fieldset" : "rz-fieldset rz-fieldset-toggleable";
    }

    /// <summary>
    /// Gets or sets a value indicating whether collapsing is allowed. Set to <c>false</c> by default.
    /// </summary>
    /// <value><c>true</c> if collapsing is allowed; otherwise, <c>false</c>.</value>
    [Parameter]
    public bool AllowCollapse { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:Radzen.Blazor.RadzenFieldset" /> is collapsed.
    /// </summary>
    /// <value><c>true</c> if collapsed; otherwise, <c>false</c>.</value>
    [Parameter]
    public bool Collapsed { get; set; }

    /// <summary>
    /// Gets or sets the title attribute of the expand button.
    /// </summary>
    /// <value>The title attribute value of the expand button.</value>
    [Parameter]
    public string ExpandTitle { get; set; }

    /// <summary>
    /// Gets or sets the title attribute of the collapse button.
    /// </summary>
    /// <value>The title attribute value of the collapse button.</value>
    [Parameter]
    public string CollapseTitle { get; set; }

    /// <summary>
    /// Gets or sets the aria-label attribute of the expand button.
    /// </summary>
    /// <value>The aria-label attribute value of the expand button.</value>
    [Parameter]
    public string ExpandAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the aria-label attribute of the collapse button.
    /// </summary>
    /// <value>The aria-label attribute value of the collapse button.</value>
    [Parameter]
    public string CollapseAriaLabel { get; set; }

    /// <summary>Gets or sets the icon.</summary>
    /// <value>The icon.</value>
    [Parameter]
    public string Icon { get; set; }

    /// <summary>Gets or sets the icon color.</summary>
    /// <value>The icon color.</value>
    [Parameter]
    public string IconColor { get; set; }

    /// <summary>Gets or sets the text.</summary>
    /// <value>The text.</value>
    [Parameter]
    public string Text { get; set; } = "";

    /// <summary>Gets or sets the header template.</summary>
    /// <value>The header template.</value>
    [Parameter]
    public RenderFragment HeaderTemplate { get; set; }

    /// <summary>Gets or sets the child content.</summary>
    /// <value>The child content.</value>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    /// <summary>Gets or sets the summary template.</summary>
    /// <value>The summary template.</value>
    [Parameter]
    public RenderFragment SummaryTemplate { get; set; }

    /// <summary>Gets or sets the expand callback.</summary>
    /// <value>The expand callback.</value>
    [Parameter]
    public EventCallback Expand { get; set; }

    /// <summary>Gets or sets the collapse callback.</summary>
    /// <value>The collapse callback.</value>
    [Parameter]
    public EventCallback Collapse { get; set; }

    private async Task Toggle(EventArgs args)
    {
        RenamerFieldSet radzenFieldset = this;
        radzenFieldset.collapsed = !radzenFieldset.collapsed;
        radzenFieldset.contentStyle = radzenFieldset.collapsed ? "display: none;" : "";
        radzenFieldset.summaryContentStyle = !radzenFieldset.collapsed ? "display: none" : "";
        if (radzenFieldset.collapsed)
            await radzenFieldset.Collapse.InvokeAsync((object)args);
        else
            await radzenFieldset.Expand.InvokeAsync((object)args);
        radzenFieldset.StateHasChanged();
    }

    /// <inheritdoc />
    protected override void OnInitialized() => this.collapsed = this.Collapsed;

    private string TitleAttribute()
    {
        return this.collapsed
                ? (!string.IsNullOrWhiteSpace(this.ExpandTitle) ? this.ExpandTitle : "Expand")
                : (!string.IsNullOrWhiteSpace(this.CollapseTitle) ? this.CollapseTitle : "Collapse");
    }

    private string AriaLabelAttribute()
    {
        return this.collapsed
                ? (!string.IsNullOrWhiteSpace(this.ExpandAriaLabel) ? this.ExpandAriaLabel : "Expand")
                : (!string.IsNullOrWhiteSpace(this.CollapseAriaLabel) ? this.CollapseAriaLabel : "Collapse");
    }

    /// <inheritdoc />
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.DidParameterChange<bool>("Collapsed", this.Collapsed))
            this.collapsed = parameters.GetValueOrDefault<bool>("Collapsed");
        await base.SetParametersAsync(parameters);
    }

    /// <inheritdoc />
    protected override Task OnParametersSetAsync()
    {
        this.contentStyle = this.collapsed ? "display: none;" : "";
        this.summaryContentStyle = !this.collapsed ? "display: none" : "";
        return base.OnParametersSetAsync();
    }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (!this.Visible)
            return;
        __builder.AddContent(0, "    ");
        __builder.OpenElement(1, "fieldset");
        __builder.AddMultipleAttributes(2,
                RuntimeHelpers.TypeCheck<IEnumerable<KeyValuePair<string, object>>>(
                        (IEnumerable<KeyValuePair<string, object>>)this.Attributes));
        __builder.AddAttribute(3, "class", this.GetCssClass());
        __builder.AddAttribute(4, "style", this.Style);
        __builder.AddAttribute(5, "id", this.GetId());
        __builder.AddElementReferenceCapture(6, (Action<ElementReference>)(__value => this.Element = __value));
        __builder.AddMarkupContent(7, "\r\n");
        if (this.AllowCollapse || !string.IsNullOrEmpty(this.Text) || !string.IsNullOrEmpty(this.Icon) ||
            this.HeaderTemplate != null)
        {
            __builder.AddContent(8, "        ");
            __builder.OpenElement(9, "legend");
            __builder.AddAttribute(10, "class", "rz-fieldset-legend");
            __builder.AddAttribute(11, "style", "white-space:nowrap");
            __builder.AddMarkupContent(12, "\r\n\r\n");
            if (this.AllowCollapse)
            {
                __builder.AddContent(13, "                ");
                __builder.OpenElement(14, "a");
                __builder.AddAttribute(15, "title", this.TitleAttribute());
                __builder.AddAttribute(16, "aria-label", this.AriaLabelAttribute());
                __builder.AddEventPreventDefaultAttribute(17, "onclick", true);
                __builder.AddAttribute(18, "aria-controls", "rz-fieldset-0-content");
                __builder.AddAttribute(19, "aria-expanded", "false");
                __builder.AddAttribute<MouseEventArgs>(20, "onclick",
                        EventCallback.Factory.Create<MouseEventArgs>((object)this,
                                new Func<MouseEventArgs, Task>(this.Toggle)));
                __builder.AddMarkupContent(21, "\r\n");
                if (this.collapsed)
                    __builder.AddMarkupContent(22,
                            "                    <span class=\"rz-fieldset-toggler rzi rzi-w rzi-plus\"></span>\r\n");
                else
                    __builder.AddMarkupContent(23,
                            "                    <span class=\"rz-fieldset-toggler rzi rzi-w rzi-minus\"></span>\r\n");
                __builder.AddMarkupContent(24, "\r\n");
                if (!string.IsNullOrEmpty(this.Icon))
                {
                    __builder.AddContent(25, "                    ");
                    __builder.OpenElement(26, "i");
                    __builder.AddAttribute(27, "class", "rzi");
                    __builder.AddContent(28, (MarkupString)this.Icon);
                    __builder.CloseElement();
                    __builder.OpenElement(29, "span");
                    __builder.AddContent(30, this.Text);
                    __builder.CloseElement();
                    __builder.AddMarkupContent(31, "\r\n");
                }
                else
                {
                    __builder.AddContent(32, "                    ");
                    __builder.OpenElement(33, "span");
                    __builder.AddAttribute(34, "class", "rz-fieldset-legend-text");
                    __builder.AddContent(35, this.Text);
                    __builder.CloseElement();
                    __builder.AddMarkupContent(36, "\r\n");
                }

                __builder.AddContent(37, "                ");
                __builder.AddContent(38, this.HeaderTemplate);
                __builder.AddMarkupContent(39, "\r\n                ");
                __builder.CloseElement();
                __builder.AddMarkupContent(40, "\r\n");
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Icon))
                {
                    __builder.AddContent(41, "                    ");
                    __builder.OpenElement(42, "i");
                    __builder.AddAttribute(43, "class", "rzi");
                    __builder.AddAttribute(44, "style",
                            !string.IsNullOrEmpty(this.IconColor) ? "color:" + this.IconColor : (string)null);
                    __builder.AddContent(45, (MarkupString)this.Icon);
                    __builder.CloseElement();
                    __builder.OpenElement(46, "span");
                    __builder.AddContent(47, this.Text);
                    __builder.CloseElement();
                    __builder.AddMarkupContent(48, "\r\n");
                }
                else
                {
                    __builder.AddContent(49, "                    ");
                    __builder.OpenElement(50, "span");
                    __builder.AddAttribute(51, "class", "rz-fieldset-legend-text");
                    __builder.AddContent(52, this.Text);
                    __builder.CloseElement();
                    __builder.AddMarkupContent(53, "\r\n");
                }

                __builder.AddContent(54, this.HeaderTemplate);
            }

            __builder.AddContent(55, "        ");
            __builder.CloseElement();
            __builder.AddMarkupContent(56, "\r\n");
        }

        __builder.AddContent(57, "        ");
        __builder.OpenElement(58, "div");
        __builder.AddAttribute(59, "class", "rz-fieldset-content-wrapper");
        __builder.AddAttribute(60, "role", "region");
        __builder.AddAttribute(61, "id", "rz-fieldset-0-content");
        __builder.AddAttribute(62, "aria-hidden", "false");
        __builder.AddAttribute(63, "style", this.contentStyle);
        __builder.AddMarkupContent(64, "\r\n            ");
        __builder.OpenElement(65, "div");
        __builder.AddAttribute(66, "class", "rz-fieldset-content");
        __builder.AddAttribute(67, "style", "padding: .5rem;");
        __builder.AddMarkupContent(68, "\r\n                ");
        __builder.AddContent(69, this.ChildContent);
        __builder.AddMarkupContent(70, "\r\n            ");
        __builder.CloseElement();
        __builder.AddMarkupContent(71, "\r\n        ");
        __builder.CloseElement();
        __builder.AddMarkupContent(72, "\r\n");
        if (this.SummaryTemplate != null)
        {
            __builder.AddContent(73, "            ");
            __builder.OpenElement(74, "div");
            __builder.AddAttribute(75, "class", "rz-fieldset-content-wrapper");
            __builder.AddAttribute(76, "role", "region");
            __builder.AddAttribute(77, "aria-hidden", "false");
            __builder.AddAttribute(78, "style", this.summaryContentStyle);
            __builder.AddMarkupContent(79, "\r\n                ");
            __builder.OpenElement(80, "div");
            __builder.AddAttribute(81, "class", "rz-fieldset-content rz-fieldset-content-summary");
            __builder.AddMarkupContent(82, "\r\n                    ");
            __builder.AddContent(83, this.SummaryTemplate);
            __builder.AddMarkupContent(84, "\r\n                ");
            __builder.CloseElement();
            __builder.AddMarkupContent(85, "\r\n            ");
            __builder.CloseElement();
            __builder.AddMarkupContent(86, "\r\n");
        }

        __builder.AddContent(87, "    ");
        __builder.CloseElement();
        __builder.AddMarkupContent(88, "\r\n");
    }
}