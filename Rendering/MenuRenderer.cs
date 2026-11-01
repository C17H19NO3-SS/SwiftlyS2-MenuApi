using System.Text;
using MenuApi.API;
using MenuApi.Items;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Rendering;

public static class MenuRenderer
{
    public static string Render(
        IPlayer player,
        IMenu menu,
        int pageIndex,
        bool hasPreviousPage,
        bool hasNextPage,
        bool hasParentMenu)
    {
        var sb = new StringBuilder(1024);
        var theme = menu.Theme ?? MenuTheme.Default;

        int itemsPerPage = Math.Clamp(menu.ItemsPerPage, 1, 7);
        int totalItems = menu.Items.Count;
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)itemsPerPage));
        int currentPage = Math.Clamp(pageIndex, 0, totalPages - 1);

        // Ana sarmalayıcı (en dış eleman)
        sb.Append("<div>");

        string titleFontAttr = theme.GetTitleFontAttribute();
        string itemFontAttr = theme.GetItemFontAttribute();
        string subtitleFontAttr = theme.GetSubtitleFontAttribute();

        // 1. Başlık
        sb.Append("<span ").Append(titleFontAttr).Append(" color=\"").Append(theme.TitleColor).Append("\">");
        sb.Append(theme.TitlePrefix);
        sb.Append(EscapeHtml(menu.Title));
        sb.Append(theme.TitleSuffix);
        sb.Append("</span><br>");

        // 2. Alt başlık
        if (!string.IsNullOrWhiteSpace(menu.Subtitle))
        {
            sb.Append("<span ").Append(subtitleFontAttr).Append(" color=\"").Append(theme.SubtitleColor).Append("\">");
            sb.Append(EscapeHtml(menu.Subtitle));
            sb.Append("</span><br>");
        }

        // 3. 1-7 Seçenekleri
        int startIndex = currentPage * itemsPerPage;
        int endIndex = Math.Min(startIndex + itemsPerPage, totalItems);

        int slotNumber = 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            var item = menu.Items[i];

            if (!item.IsSelectable)
            {
                // Ayraç veya bilgilendirme metni
                string color = !string.IsNullOrEmpty(item.CustomColor) ? item.CustomColor : theme.InfoTextColor;
                sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(color).Append("\">");
                sb.Append(item.GetDisplayText(player, slotNumber, false));
                sb.Append("</span><br>");
            }
            else if (!item.IsEnabled)
            {
                // Devre dışı / Kilitli öğe
                sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.DisabledTextColor).Append("\">");
                sb.Append(slotNumber);
                sb.Append(". ");
                sb.Append(item.GetDisplayText(player, slotNumber, false));
                sb.Append("</span><br>");
                slotNumber++;
            }
            else
            {
                // Aktif öğe: Slot numarası + metin
                string textColor = !string.IsNullOrEmpty(item.CustomColor) ? item.CustomColor : theme.ItemTextColor;
                sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.SlotNumberColor).Append("\">");
                sb.Append(slotNumber);
                sb.Append(". </span><span ").Append(itemFontAttr).Append(" color=\"").Append(textColor).Append("\">");
                sb.Append(item.GetDisplayText(player, slotNumber, false));
                sb.Append("</span><br>");
                slotNumber++;
            }
        }

        // 4. Çok sayfalı ise sayfa bilgisi: Sayfa 1/2
        if (totalPages > 1)
        {
            sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.PageInfoColor).Append("\">Sayfa ");
            sb.Append(currentPage + 1);
            sb.Append("/");
            sb.Append(totalPages);
            sb.Append("</span><br>");
        }

        // 5. 8. Slot: Geri
        bool canGoBack = hasPreviousPage || hasParentMenu;
        if (canGoBack)
        {
            sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.SlotNumberColor).Append("\">8. </span><span ").Append(itemFontAttr).Append(" color=\"").Append(theme.BackActiveColor).Append("\">").Append(theme.BackText).Append("</span><br>");
        }
        else
        {
            sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.BackDisabledColor).Append("\">8. ").Append(theme.BackText).Append("</span><br>");
        }

        // 6. 9. Slot: İleri (Alt satıra geçmesi için <br> eklendi)
        if (hasNextPage)
        {
            sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.SlotNumberColor).Append("\">9. </span><span ").Append(itemFontAttr).Append(" color=\"").Append(theme.NextActiveColor).Append("\">").Append(theme.NextText).Append("</span><br>");
        }
        else
        {
            sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.NextDisabledColor).Append("\">9. ").Append(theme.NextText).Append("</span><br>");
        }

        // 7. 0. Slot: Çıkış (Artık 9'un hemen altında, ayrı bir satırda yer alır)
        sb.Append("<span ").Append(itemFontAttr).Append(" color=\"").Append(theme.ExitSlotColor).Append("\">0. </span><span ").Append(itemFontAttr).Append(" color=\"").Append(theme.ExitColor).Append("\">").Append(theme.ExitText).Append("</span><br>");

        sb.Append("</div>");

        return sb.ToString();
    }

    private static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;");
    }
}
