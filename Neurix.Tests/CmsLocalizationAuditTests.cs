using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Neurix.Localization;
using Xunit;
using Xunit.Abstractions;

namespace Neurix.Tests
{
    public class CmsLocalizationAuditTests
    {
        private readonly ITestOutputHelper _output;

        public CmsLocalizationAuditTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AuditAllViewsAndModelsForMissingArabicTranslations()
        {
            // Resolve project paths
            var baseDir = AppContext.BaseDirectory;
            var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var neurixDir = Path.Combine(solutionDir, "Neurix");
            var arJsonPath = Path.Combine(neurixDir, "Resources", "Cms", "cms.ar.json");

            Assert.True(File.Exists(arJsonPath), $"cms.ar.json not found at {arJsonPath}");

            var jsonContent = File.ReadAllText(arJsonPath);
            var arabicDict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
                ?? new Dictionary<string, string>();

            _output.WriteLine($"Loaded {arabicDict.Count} keys from cms.ar.json");

            // 1. Scan all Razor Views (excluding Views/Home which uses client-side data-en/data-ar)
            var viewsDir = Path.Combine(neurixDir, "Views");
            var cshtmlFiles = Directory.GetFiles(viewsDir, "*.cshtml", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.Combine("Views", "Home")))
                .ToList();

            var missingViewKeys = new Dictionary<string, List<string>>();

            var lRegex = new Regex(@"L\[""([^""]+)""\]");
            var lFmtRegex = new Regex(@"L\.Format\(""([^""]+)""");
            var lSingleRegex = new Regex(@"L\['([^']+)'\]");

            foreach (var file in cshtmlFiles)
            {
                var text = File.ReadAllText(file);
                var relPath = Path.GetRelativePath(viewsDir, file);

                void CheckKey(string k)
                {
                    if (!arabicDict.ContainsKey(k) || string.IsNullOrWhiteSpace(arabicDict[k]))
                    {
                        if (!missingViewKeys.ContainsKey(k))
                            missingViewKeys[k] = new List<string>();
                        if (!missingViewKeys[k].Contains(relPath))
                            missingViewKeys[k].Add(relPath);
                    }
                }

                foreach (Match m in lRegex.Matches(text)) CheckKey(m.Groups[1].Value);
                foreach (Match m in lFmtRegex.Matches(text)) CheckKey(m.Groups[1].Value);
                foreach (Match m in lSingleRegex.Matches(text)) CheckKey(m.Groups[1].Value);
            }

            // 2. Scan C# Model files
            var modelDirs = new[]
            {
                Path.Combine(neurixDir, "Models"),
                Path.Combine(solutionDir, "Neurix.BLL", "Dtos")
            };

            var missingModelKeys = new Dictionary<string, List<string>>();
            var nameAttrRegex = new Regex(@"Name\s*=\s*""([^""]+)""");
            var errAttrRegex = new Regex(@"ErrorMessage\s*=\s*""([^""]+)""");

            foreach (var dir in modelDirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(file);
                    var fileName = Path.GetFileName(file);

                    void CheckModelKey(string k)
                    {
                        if (!arabicDict.ContainsKey(k) || string.IsNullOrWhiteSpace(arabicDict[k]))
                        {
                            if (!missingModelKeys.ContainsKey(k))
                                missingModelKeys[k] = new List<string>();
                            if (!missingModelKeys[k].Contains(fileName))
                                missingModelKeys[k].Add(fileName);
                        }
                    }

                    foreach (Match m in nameAttrRegex.Matches(text)) CheckModelKey(m.Groups[1].Value);
                    foreach (Match m in errAttrRegex.Matches(text)) CheckModelKey(m.Groups[1].Value);
                }
            }

            // 3. Scan Controllers for TempData messages
            var controllersDir = Path.Combine(neurixDir, "Controllers");
            var missingControllerKeys = new Dictionary<string, List<string>>();
            var tempDataRegex = new Regex(@"TempData\[""(?:Success|Error|Warning|Info)""\]\s*=\s*""([^""]+)""");

            if (Directory.Exists(controllersDir))
            {
                foreach (var file in Directory.GetFiles(controllersDir, "*.cs", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(file);
                    var fileName = Path.GetFileName(file);

                    foreach (Match m in tempDataRegex.Matches(text))
                    {
                        var k = m.Groups[1].Value;
                        if (!arabicDict.ContainsKey(k) || string.IsNullOrWhiteSpace(arabicDict[k]))
                        {
                            if (!missingControllerKeys.ContainsKey(k))
                                missingControllerKeys[k] = new List<string>();
                            if (!missingControllerKeys[k].Contains(fileName))
                                missingControllerKeys[k].Add(fileName);
                        }
                    }
                }
            }

            var reportPath = Path.Combine(solutionDir, "Neurix_Localization_Missing_Report.json");
            var reportObj = new
            {
                TotalArabicDictionaryKeys = arabicDict.Count,
                MissingViewKeysCount = missingViewKeys.Count,
                MissingViewKeys = missingViewKeys,
                MissingModelKeysCount = missingModelKeys.Count,
                MissingModelKeys = missingModelKeys,
                MissingControllerKeysCount = missingControllerKeys.Count,
                MissingControllerKeys = missingControllerKeys
            };

            File.WriteAllText(reportPath, JsonSerializer.Serialize(reportObj, new JsonSerializerOptions { WriteIndented = true }));
            _output.WriteLine($"Report written to {reportPath}. Missing views: {missingViewKeys.Count}, Missing models: {missingModelKeys.Count}, Missing controllers: {missingControllerKeys.Count}");

            Assert.Empty(missingViewKeys);
            Assert.Empty(missingModelKeys);
            Assert.Empty(missingControllerKeys);
        }

        [Fact]
        public void ScanViewsForHardcodedEnglishTextWithoutL()
        {
            var baseDir = AppContext.BaseDirectory;
            var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var neurixDir = Path.Combine(solutionDir, "Neurix");
            var viewsDir = Path.Combine(neurixDir, "Views");

            var cshtmlFiles = Directory.GetFiles(viewsDir, "*.cshtml", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.Combine("Views", "Home")))
                .ToList();

            var findings = new List<string>();

            // Regex to find HTML tags containing plain English letters without Razor @
            // e.g. <h1...>Some English Text</h1> or <p...>English</p> or <button...>English</button>
            var textRegex = new Regex(@">([^<@\r\n{}]*[A-Za-z]{3,}[^<@\r\n{}]*)<");

            foreach (var file in cshtmlFiles)
            {
                var text = File.ReadAllText(file);
                var relPath = Path.GetRelativePath(viewsDir, file);

                // remove razor comments
                text = Regex.Replace(text, @"@\*.*?\*@", "", RegexOptions.Singleline);
                // remove <script> blocks
                text = Regex.Replace(text, @"<script\b[^>]*>.*?</script>", "", RegexOptions.Singleline);
                // remove <style> blocks
                text = Regex.Replace(text, @"<style\b[^>]*>.*?</style>", "", RegexOptions.Singleline);

                var lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("@") || line.StartsWith("//")) continue;

                    foreach (Match m in textRegex.Matches(line))
                    {
                        var content = m.Groups[1].Value.Trim();
                        // Filter out common false positives (like classes, lucide icon names, urls, variables)
                        if (string.IsNullOrWhiteSpace(content)) continue;
                        if (content.StartsWith("http") || content.StartsWith("/") || content.StartsWith("#")) continue;
                        if (content.All(c => char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c)))
                        {
                            // If it doesn't contain @L[ or @Model or @ViewBag or @...
                            if (!content.Contains("@") && content.Length > 2)
                            {
                                findings.Add($"{relPath}:{i + 1} -> \"{content}\"");
                            }
                        }
                    }
                }
            }

            var hardcodedReportPath = Path.Combine(solutionDir, "Neurix_Hardcoded_Strings_Report.txt");
            File.WriteAllLines(hardcodedReportPath, findings);
            _output.WriteLine($"Found {findings.Count} potential hardcoded strings. Report written to {hardcodedReportPath}");
        }

        [Fact]
        public void ApplyAndVerifyArabicTranslations()
        {
            var baseDir = AppContext.BaseDirectory;
            var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var neurixDir = Path.Combine(solutionDir, "Neurix");
            var arJsonPath = Path.Combine(neurixDir, "Resources", "Cms", "cms.ar.json");

            var existingJson = File.ReadAllText(arJsonPath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(existingJson)
                ?? new Dictionary<string, string>();

            var newEntries = new Dictionary<string, string>
            {
                // Navigation Menus & Placement
                ["#divisions (Anchor)"] = "قسم الأقسام (رابط داخلي #divisions)",
                ["#pillars (Anchor)"] = "قسم الركائز (رابط داخلي #pillars)",
                ["#services (Anchor)"] = "قسم الخدمات (رابط داخلي #services)",
                ["About Us (/Home/About)"] = "من نحن (/Home/About)",
                ["Add a new navigation link to the website Header, Footer, or Legal Bottom bar."] = "إضافة رابط تنقل جديد إلى ترويسة الموقع، أو التذييل، أو الشريط السفلي القانوني.",
                ["Add First Menu Item"] = "إضافة أول عنصر قائمة",
                ["Add Menu Item"] = "إضافة عنصر قائمة",
                ["All Items"] = "جميع العناصر",
                ["Arabic label cannot exceed 100 characters."] = "لا يمكن أن تتجاوز التسمية بالعربية 100 حرف.",
                ["Arabic label is required."] = "التسمية بالعربية مطلوبة.",
                ["Are you sure you want to delete this menu item?"] = "هل أنت متأكد من حذف عنصر القائمة هذا؟",
                ["Back to Menus"] = "رجوع إلى القوائم",
                ["Bilingual Menu Labels"] = "تسميات القائمة ثنائية اللغة",
                ["Both"] = "كلاهما",
                ["Both Header & Footer"] = "الترويسة والتذييل معًا",
                ["Bottom Bar"] = "الشريط السفلي",
                ["Brand & Placement Settings"] = "إعدادات العلامة والموضع",
                ["Careers / News (/Home/ComingSoon)"] = "الوظائف / الأخبار (/Home/ComingSoon)",
                ["Club (/Home/Club)"] = "النادي (/Home/Club)",
                ["Contact Us (/Home/Contact)"] = "تواصل معنا (/Home/Contact)",
                ["Controls live visibility on the website header/footer."] = "يتحكم في الظهور المباشر على ترويسة أو تذييل الموقع.",
                ["Create Menu Item"] = "إنشاء عنصر قائمة",
                ["Edit Menu Item"] = "تعديل عنصر القائمة",
                ["English label cannot exceed 100 characters."] = "لا يمكن أن تتجاوز التسمية بالإنجليزية 100 حرف.",
                ["English label is required."] = "التسمية بالإنجليزية مطلوبة.",
                ["Footer Bottom Bar (Legal/Security)"] = "شريط التذييل السفلي (قانوني / أمان)",
                ["Footer Links"] = "روابط التذييل",
                ["Get started by adding custom navigation links for your website header and footer."] = "ابدأ بإضافة روابط تنقل مخصصة لترويسة وتذييل موقعك.",
                ["Header"] = "الترويسة",
                ["Header Only"] = "الترويسة فقط",
                ["Home (/)"] = "الرئيسية (/)",
                ["HQ (/Home/HQ)"] = "المقر الرئيسي (/Home/HQ)",
                ["Icon Name (Lucide)"] = "اسم أيقونة Lucide",
                ["Label (EN / AR)"] = "التسمية (بالإنجليزية / بالعربية)",
                ["Labs (/Home/Labs)"] = "المختبرات (/Home/Labs)",
                ["Legal / Bottom"] = "قانوني / الشريط السفلي",
                ["Manage custom navigation items for the website Header, Footer columns, and Bottom legal bar."] = "إدارة عناصر التنقل المخصصة لترويسة الموقع، وأعمدة التذييل، والشريط السفلي القانوني.",
                ["Menu Label (AR)"] = "تسمية القائمة (بالعربية)",
                ["Menu Label (EN)"] = "تسمية القائمة (بالإنجليزية)",
                ["Modify navigation label, target URL, placement, display order, or visibility."] = "تعديل تسمية الرابط، أو العنوان المستهدف، أو الموضع، أو ترتيب العرض، أو حالة الظهور.",
                ["Navigation"] = "التنقل",
                ["Navigation Menus"] = "قوائم التنقل",
                ["No menu items found"] = "لم يتم العثور على عناصر قائمة",
                ["Open in New Tab"] = "فتح في علامة تبويب جديدة",
                ["Opens in new tab"] = "يفتح في علامة تبويب جديدة",
                ["Opens link in a new browser tab (target='_blank')."] = "يفتح الرابط في علامة تبويب جديدة بالمتصفح (target='_blank').",
                ["Placement"] = "الموضع",
                ["Placement Location"] = "موضع العرض",
                ["Placement location is required."] = "موضع العرض مطلوب.",
                ["Plus (/Home/Plus)"] = "بلس (/Home/Plus)",
                ["Privacy Policy (/Home/Privacy)"] = "سياسة الخصوصية (/Home/Privacy)",
                ["Profile:"] = "الملف:",
                ["Published on Live Site"] = "منشور في الموقع المباشر",
                ["Target URL & Route"] = "الرابط المستهدف والمسار",
                ["Target URL / Route"] = "الرابط المستهدف / المسار",
                ["Technology (/Home/Technology)"] = "التقنية (/Home/Technology)",
                ["URL / Route is required."] = "الرابط / المسار مطلوب.",
                ["URL / Route path is required."] = "مسار الرابط / المسار مطلوب.",
                ["URL cannot exceed 500 characters."] = "لا يمكن أن يتجاوز الرابط 500 حرف.",
                ["Visibility & Navigation Behavior"] = "الظهور وسلوك التنقل",

                // Homepage Sections Index & Cards
                ["Homepage Sections"] = "أقسام الصفحة الرئيسية",
                ["Homepage Sections Management"] = "إدارة أقسام الصفحة الرئيسية",
                ["Manage dynamic copy, CTA buttons, metrics, capability pillars, and illustrations for key sections of the homepage."] = "إدارة النصوص الديناميكية، وأزرار الحث على اتخاذ إجراء (CTA)، والمؤشرات الإحصائية، والركائز، والرسومات التوضيحية للأقسام الرئيسية في الصفحة الأولى.",
                ["Filter Profile:"] = "تصفية حسب الملف:",
                ["Live"] = "منشور",
                ["Draft"] = "مسودة",
                ["Hero Banner"] = "القسم الترويجي الرئيسي (Hero)",
                ["Main hero headline, dynamic glowing keyword, dual action buttons (CTAs), and 3 statistic counters."] = "العنوان الرئيسي، والكلمة المضيئة البارزة، وزرا الإجراء (CTAs)، وثلاثة عدادات إحصائية.",
                ["Badge:"] = "الشارة:",
                ["Headline (EN):"] = "العنوان (بالإنجليزية):",
                ["Headline (AR):"] = "العنوان (بالعربية):",
                ["Stats:"] = "الإحصائيات:",
                ["Edit Hero Section"] = "تعديل القسم الترويجي (Hero)",
                ["Edit Hero Banner Section"] = "تعديل القسم الترويجي الرئيسي",
                ["Human Vision"] = "الرؤية الإنسانية",
                ["Core philosophy narrative, dual-paragraph message on purposeful tech, and side illustration."] = "السرد الفلسفي الجوهري، ورسالة ثنائية الفقرات حول التكنولوجيا الهادفة، والرسم التوضيحي الجانبي.",
                ["Title (EN):"] = "العنوان (بالإنجليزية):",
                ["Title (AR):"] = "العنوان (بالعربية):",
                ["Image:"] = "الصورة:",
                ["Images:"] = "الصور:",
                ["Edit Human Vision"] = "تعديل الرؤية الإنسانية",
                ["Edit Human Vision Section"] = "تعديل قسم الرؤية الإنسانية",
                ["Message to Pioneers"] = "رسالة إلى رواد الابتكار",
                ["Inspirational manifesto for engineers, programmers, and creators with custom image."] = "بيان ملهم للمهندسين والمبرمجين والمبتكرين مصحوب بصورة مخصصة.",
                ["Edit Pioneers Section"] = "تعديل قسم رواد الابتكار",
                ["Edit Message to Pioneers Section"] = "تعديل قسم رسالة إلى رواد الابتكار",
                ["Pillars"] = "الركائز",
                ["Core Capabilities"] = "القدرات والركائز الأساسية",
                ["Core Capabilities Section"] = "قسم القدرات والركائز الأساسية",
                ["Modular capability cards with custom icons (Advanced AI, Digital Platforms, Security, etc.)"] = "بطاقات قدرات معيارية بأيقونات مخصصة (الذكاء الاصطناعي المتقدم، المنصات الرقمية، الأمان، وغيرها).",
                ["Title (EN/AR):"] = "العنوان (إنجليزي / عربي):",
                ["Active Cards:"] = "البطاقات النشطة:",
                ["No cards added yet."] = "لم تتم إضافة بطاقات بعد.",
                ["Manage Capabilities & Cards"] = "إدارة القدرات والبطاقات",
                ["Vision Framework"] = "إطار عمل الرؤية والتنفيذ",
                ["Vision & Implementation Framework"] = "إطار عمل الرؤية والتنفيذ",
                ["Edit Vision Framework"] = "تعديل إطار عمل الرؤية والتنفيذ",
                ["Edit Vision & Implementation Framework"] = "تعديل إطار عمل الرؤية والتنفيذ",
                ["Operational strategy narrative, live service matrix, and dual overlapping tech images."] = "سرد الاستراتيجية التشغيلية، ومصفوفة الخدمات، وصورتان تقنيتان متداخلتان.",
                ["Headline:"] = "العنوان الرئيسي:",
                ["Call To Action Banner"] = "شريط الحث على التواصل (CTA)",
                ["Manage the bottom call to action banner, headline, contact email, and CTA button."] = "إدارة شريط الحث على اتخاذ إجراء السفلي، والعنوان، والبريد الإلكتروني للتواصل، وزر الحث على التفاعل.",
                ["Manage the bottom call to action banner, headline, contact email, button CTA, and background texture."] = "إدارة شريط الحث على اتخاذ إجراء، والعنوان، وبريد التواصل، وزر الإجراء، وصورة الخلفية.",
                ["Button:"] = "الزر:",
                ["Edit Call To Action"] = "تعديل شريط الحث على التواصل",
                ["Edit Call To Action Section"] = "تعديل قسم شريط الحث على التواصل",

                // Hero Form
                ["Manage the headline, glowing highlighted keyword, subtitle, dual action buttons, and statistic metrics."] = "إدارة العنوان الرئيسي، والكلمة المضيئة البارزة، والعنوان الفرعي، وزري الإجراء، والمؤشرات الإحصائية.",
                ["Main Headline (with Highlighted Glowing Word)"] = "العنوان الرئيسي (مع الكلمة المضيئة البارزة)",
                ["Three-part breakdown for gradient rendering"] = "تقسيم ثلاثي الأجزاء لعرض التدرج اللوني",
                ["English Headline:"] = "العنوان بالإنجليزية:",
                ["Arabic Headline:"] = "العنوان بالعربية:",
                ["Prefix"] = "البادئة",
                ["Suffix"] = "اللاحقة",
                ["Highlight (Glowing)"] = "الكلمة المضيئة",
                ["Subtitle & Description"] = "العنوان الفرعي والوصف",
                ["Action Buttons (CTAs)"] = "أزرار الإجراء (CTAs)",
                ["Primary Button (Glowing Gradient)"] = "الزر الأساسي (بتدرج مضيء)",
                ["Secondary Button (Outlined)"] = "الزر الثانوي (بإطار)",
                ["Bottom Statistics / Metrics Cards"] = "بطاقات الإحصائيات والمؤشرات السفلية",
                ["Metric 1"] = "المؤشر الأول",
                ["Metric 2"] = "المؤشر الثاني",
                ["Metric 3"] = "المؤشر الثالث",
                ["Published on Live Website"] = "منشور على الموقع المباشر",
                ["Save Hero Changes"] = "حفظ تعديلات القسم الترويجي",

                // Human Vision & Pioneers Forms
                ["Manage core philosophy message, dual paragraphs on human-centric tech, and side illustration."] = "إدارة رسالة الفلسفة الجوهرية، والفقرتين حول التقنية المرتكزة على الإنسان، والرسم التوضيحي الجانبي.",
                ["Section Badge & Heading"] = "شارة القسم وعنوانه",
                ["Title in English (Prefix + Glowing Highlight):"] = "العنوان بالإنجليزية (البادئة + الكلمة البارزة):",
                ["Arabic Title (Prefix + Glowing Word):"] = "العنوان بالعربية (البادئة + الكلمة المضيئة):",
                ["Arabic Title (Prefix + Glowing Highlight):"] = "العنوان بالعربية (البادئة + الكلمة البارزة):",
                ["Highlight (Gradient)"] = "الكلمة البارزة المضيئة",
                ["Core Narrative Paragraphs"] = "فقرات السرد الجوهري",
                ["Pioneers Narrative Paragraphs"] = "فقرات السرد لرواد الابتكار",
                ["Paragraph 1 (The Challenge / Problem Statement)"] = "الفقرة الأولى (التحدي / المشكلة)",
                ["Paragraph 1 (The Visionary Mission)"] = "الفقرة الأولى (المهمة والرؤية الملهمة)",
                ["Paragraph 2 (Our Mission / Constructive Action)"] = "الفقرة الثانية (مهمتنا / العمل البنّاء)",
                ["Paragraph 2 (The Social & Human Impact)"] = "الفقرة الثانية (الأثر الإنساني والمجتمعي)",
                ["Illustration Image Asset"] = "صورة الرسم التوضيحي",
                ["Upload New Image"] = "رفع صورة جديدة",
                ["Or Current File Path / URL:"] = "أو مسار الملف / الرابط الحالي:",
                ["Alt Text (EN)"] = "النص البديل (بالإنجليزية)",
                ["Alt Text (AR)"] = "النص البديل (بالعربية)",
                ["Save Human Vision Changes"] = "حفظ تعديلات الرؤية الإنسانية",
                ["Save Pioneers Changes"] = "حفظ تعديلات رواد الابتكار",

                // Pillars & Capabilities
                ["Manage the section header and modular capability cards (icons, titles, and descriptions)."] = "إدارة ترويسة القسم وبطاقات القدرات المعيارية (الأيقونات والعناوين والوصف).",
                ["Section Header & Introduction"] = "ترويسة القسم والمقدمة",
                ["When disabled, this section is completely hidden from the public homepage."] = "عند التعطيل، سيتم إخفاء هذا القسم بالكامل من الصفحة الرئيسية العامة.",
                ["Save Header Changes"] = "حفظ تعديلات الترويسة",
                ["Capability Cards"] = "بطاقات القدرات",
                ["cards configured"] = "بطاقة مجهزة",
                ["Add, reorder, edit, or remove modular pillar cards displayed on the homepage grid."] = "إضافة أو إعادة ترتيب أو تعديل أو حذف بطاقات الركائز المعيارية المعروضة على شبكة الصفحة الرئيسية.",
                ["Add New Capability Card"] = "إضافة بطاقة قدرة جديدة",
                ["Add Capability Card"] = "إضافة بطاقة قدرة",
                ["Edit Capability Card"] = "تعديل بطاقة القدرة",
                ["Create Capability Card"] = "إنشاء بطاقة القدرة",
                ["Update Capability Card"] = "تحديث بطاقة القدرة",
                ["Are you sure you want to delete this capability card?"] = "هل أنت متأكد من حذف بطاقة القدرة هذه؟",
                ["No capability cards configured yet. Click above to add your first pillar."] = "لم يتم إعداد أي بطاقات قدرات بعد. انقر أعلاه لإضافة ركيزتك الأولى.",
                ["Configure icon, bilingual titles, description, and display ordering."] = "تكوين الأيقونة والعناوين ثنائية اللغة والوصف وترتيب العرض.",
                ["Lucide Icon Name (e.g. brain, layers, briefcase, shield-check, cpu, zap)"] = "اسم أيقونة Lucide (مثل brain، layers، briefcase، shield-check، cpu، zap)",
                ["Lucide icon name (e.g. brain, layers, briefcase, shield-check, cpu, zap, database, terminal, sparkles)"] = "اسم أيقونة Lucide (مثل brain، layers، briefcase، shield-check، cpu، zap، database، terminal، sparkles)",
                ["Lower numbers are displayed first on the website grid."] = "الأرقام الأقل تظهر أولاً في شبكة الموقع.",
                ["Controls whether this card appears on the public website."] = "يتحكم في ظهور هذه البطاقة على الموقع العام.",
                ["Back to Pillars"] = "رجوع إلى الركائز",

                // Vision Framework (Ethics)
                ["Manage operational strategy copy, narrative description, and dual overlapping tech images."] = "إدارة نصوص الاستراتيجية التشغيلية، والوصف السردي، والصورتين التقنيتين المتداخلتين.",
                ["Section Headline"] = "عنوان القسم",
                ["Title Highlight / Glowing (EN)"] = "الكلمة البارزة المضيئة (بالإنجليزية)",
                ["Title Highlight / Glowing (AR)"] = "الكلمة البارزة المضيئة (بالعربية)",
                ["Narrative & Strategic Description"] = "الوصف السردي والاستراتيجي",
                ["Dual Overlapping Image Cards"] = "بطاقتا الصور المتداخلة",
                ["Top Image Card (Software Desk Setup)"] = "بطاقة الصورة العلوية (بيئة تطوير البرمجيات)",
                ["Bottom Image Card (Research Lab Setup)"] = "بطاقة الصورة السفلية (مختبر الأبحاث)",
                ["Upload New Top Image (PNG, JPG, WebP)"] = "رفع صورة علوية جديدة (PNG, JPG, WebP)",
                ["Upload New Bottom Image (PNG, JPG, WebP)"] = "رفع صورة سفلية جديدة (PNG, JPG, WebP)",
                ["Top Image File (Software Desk)"] = "ملف الصورة العلوية (بيئة البرمجيات)",
                ["Top Image Path / URL"] = "مسار / رابط الصورة العلوية",
                ["Top Image Alt Text (EN)"] = "النص البديل للصورة العلوية (بالإنجليزية)",
                ["Top Image Alt Text (AR)"] = "النص البديل للصورة العلوية (بالعربية)",
                ["Bottom Image File (Research Lab)"] = "ملف الصورة السفلية (مختبر الأبحاث)",
                ["Bottom Image Path / URL"] = "مسار / رابط الصورة السفلية",
                ["Bottom Image Alt Text (EN)"] = "النص البديل للصورة السفلية (بالإنجليزية)",
                ["Bottom Image Alt Text (AR)"] = "النص البديل للصورة السفلية (بالعربية)",

                // Call to Action
                ["Eyebrow Badge"] = "الشارة العلوية",
                ["Eyebrow Badge & Headline"] = "الشارة العلوية والعنوان الرئيسي",
                ["Call to Action Button & Contact Info"] = "زر الحث على التواصل ومعلومات الاتصال",
                ["Background Texture Image"] = "صورة نقش الخلفية",
                ["Background Texture Image File"] = "ملف صورة نقش الخلفية",
                ["Background Texture Image Path / URL"] = "مسار / رابط صورة نقش الخلفية",
                ["Upload New Background Image (PNG, JPG, WebP)"] = "رفع صورة خلفية جديدة (PNG, JPG, WebP)",
                ["Or Specify Background Image Path / URL"] = "أو حدد مسار / رابط صورة الخلفية",
                ["When checked, this section is rendered on the public website."] = "عند التحديد، سيتم عرض هذا القسم على الموقع العام.",

                // ViewModels, DTOs & Validation
                ["Meta Description (English)"] = "الوصف التعريفي الميتا (بالإنجليزية)",
                ["Meta Description (Arabic)"] = "الوصف التعريفي الميتا (بالعربية)",
                ["CTA Button Text (Arabic)"] = "نص زر الإجراء (بالعربية)",
                ["Badge / Eyebrow (EN)"] = "الشارة العلوية (بالإنجليزية)",
                ["Badge / Eyebrow (AR)"] = "الشارة العلوية (بالعربية)",
                ["Headline Prefix (EN)"] = "بادئة العنوان (بالإنجليزية)",
                ["Headline Highlight (EN)"] = "الكلمة البارزة في العنوان (بالإنجليزية)",
                ["Headline Suffix (EN)"] = "لاحقة العنوان (بالإنجليزية)",
                ["Headline Prefix (AR)"] = "بادئة العنوان (بالعربية)",
                ["Headline Highlight (AR)"] = "الكلمة البارزة في العنوان (بالعربية)",
                ["Headline Suffix (AR)"] = "لاحقة العنوان (بالعربية)",
                ["Subtitle (EN)"] = "العنوان الفرعي (بالإنجليزية)",
                ["Subtitle (AR)"] = "العنوان الفرعي (بالعربية)",
                ["Primary Button Text (EN)"] = "نص الزر الأساسي (بالإنجليزية)",
                ["Primary Button Text (AR)"] = "نص الزر الأساسي (بالعربية)",
                ["Primary Button Link / Action"] = "رابط / إجراء الزر الأساسي",
                ["Secondary Button Text (EN)"] = "نص الزر الثانوي (بالإنجليزية)",
                ["Secondary Button Text (AR)"] = "نص الزر الثانوي (بالعربية)",
                ["Secondary Button Link / Action"] = "رابط / إجراء الزر الثانوي",
                ["Stat 1 Value"] = "قيمة المؤشر 1",
                ["Stat 1 Label (EN)"] = "تسمية المؤشر 1 (بالإنجليزية)",
                ["Stat 1 Label (AR)"] = "تسمية المؤشر 1 (بالعربية)",
                ["Stat 2 Value"] = "قيمة المؤشر 2",
                ["Stat 2 Label (EN)"] = "تسمية المؤشر 2 (بالإنجليزية)",
                ["Stat 2 Label (AR)"] = "تسمية المؤشر 2 (بالعربية)",
                ["Stat 3 Value"] = "قيمة المؤشر 3",
                ["Stat 3 Label (EN)"] = "تسمية المؤشر 3 (بالإنجليزية)",
                ["Stat 3 Label (AR)"] = "تسمية المؤشر 3 (بالعربية)",
                ["Title Prefix (EN)"] = "بادئة العنوان (بالإنجليزية)",
                ["Title Highlight (EN)"] = "الكلمة البارزة (بالإنجليزية)",
                ["Title Prefix (AR)"] = "بادئة العنوان (بالعربية)",
                ["Title Highlight (AR)"] = "الكلمة البارزة (بالعربية)",
                ["First Paragraph (EN)"] = "الفقرة الأولى (بالإنجليزية)",
                ["First Paragraph (AR)"] = "الفقرة الأولى (بالعربية)",
                ["Second Paragraph (EN)"] = "الفقرة الثانية (بالإنجليزية)",
                ["Second Paragraph (AR)"] = "الفقرة الثانية (بالعربية)",
                ["Illustration Image File"] = "ملف الرسم التوضيحي",
                ["Illustration Image Path / URL"] = "مسار / رابط الرسم التوضيحي",
                ["Image Alt Text (EN)"] = "النص البديل للصورة (بالإنجليزية)",
                ["Image Alt Text (AR)"] = "النص البديل للصورة (بالعربية)",
                ["Section Title (EN)"] = "عنوان القسم (بالإنجليزية)",
                ["Section Title (AR)"] = "عنوان القسم (بالعربية)",
                ["Section Subtitle / Description (EN)"] = "العنوان الفرعي / الوصف للقسم (بالإنجليزية)",
                ["Section Subtitle / Description (AR)"] = "العنوان الفرعي / الوصف للقسم (بالعربية)",
                ["Title (EN)"] = "العنوان (بالإنجليزية)",
                ["Title (AR)"] = "العنوان (بالعربية)",
                ["Description (EN)"] = "الوصف (بالإنجليزية)",
                ["Description (AR)"] = "الوصف (بالعربية)",
                ["Description / Narrative (EN)"] = "الوصف / السرد (بالإنجليزية)",
                ["Description / Narrative (AR)"] = "الوصف / السرد (بالعربية)",
                ["Button Text (EN)"] = "نص الزر (بالإنجليزية)",
                ["Button Text (AR)"] = "نص الزر (بالعربية)",
                ["Button URL"] = "رابط الزر",
                ["Company Brand Profile"] = "ملف العلامة التجارية للشركة",
                ["Company profile is required."] = "ملف الشركة مطلوب.",
                ["Invalid phone number format."] = "صيغة رقم الهاتف غير صحيحة.",
                ["Invalid website URL."] = "رابط الموقع غير صحيح.",
                ["Invalid Github URL."] = "رابط GitHub غير صحيح.",
                ["Platform name is required (e.g., linkedin, twitter, facebook)."] = "اسم المنصة مطلوب (مثل linkedin، twitter، facebook).",
                ["URL is required."] = "الرابط مطلوب.",
                ["Please enter a valid URL."] = "يرجى إدخال رابط صحيح.",

                // Shell & Other views
                ["Back to Sections"] = "رجوع إلى الأقسام",
                ["Cover Image & Media Assets"] = "صورة الغلاف والوسائط",
                ["Division cover image preview"] = "معاينة صورة غلاف القسم",
                ["Or enter path / URL (e.g. /images/division-cover.png)"] = "أو أدخل المسار / الرابط (مثل /images/division-cover.png)",
                ["Mission & Vision"] = "الرسالة والرؤية",
                ["CMS Overview"] = "نظرة عامة على CMS",
                ["Pub"] = "منشور",
                ["English text"] = "نص إنجليزي",
                ["Switch to CRM"] = "التبديل إلى CRM",
                ["Switch to CMS"] = "التبديل إلى CMS",
                ["View Public Website"] = "عرض الموقع العام",
                ["Public Site"] = "الموقع العام",
                ["Log out"] = "تسجيل الخروج",
                ["Toggle navigation"] = "تبديل القائمة",
                ["Toggle theme"] = "تبديل المظهر",
                ["Live Page Blocks & Ecosystem Sections"] = "أقسام الصفحة المباشرة ووحدات المنظومة",
                ["Additional sections visibly published on the About page beyond the hero and mission."] = "أقسام إضافية منشورة على صفحة من نحن بالإضافة إلى عنوان الغلاف والرسالة.",
                ["View in Page Sections"] = "عرض في أقسام الصفحات",
                ["Principles Band"] = "قسم المبادئ",
                ["What We Stand For"] = "ما نؤمن به",
                ["5 core principle cards defining organizational values."] = "5 بطاقات مبادئ أساسية تحدد القيم المؤسسية.",
                ["Edit Principles"] = "تعديل المبادئ",
                ["Structure Band"] = "قسم الهيكل التنظيمي",
                ["Organizational Structure"] = "الهيكل التنظيمي",
                ["5 subsidiary division cards and direct links."] = "5 بطاقات للأقسام التابعة وروابطها المباشرة.",
                ["Edit Structure"] = "تعديل الهيكل التنظيمي",
                ["Leadership & Researchers"] = "القيادة والباحثون",
                ["Band headline copy (members edited under Team Members)."] = "نصوص ترويسة القسم (يتم تعديل الأعضاء من قسم أعضاء الفريق).",
                ["Edit Leadership Band"] = "تعديل قسم القيادة",
                ["Live Homepage Placement:"] = "موضع العرض على الصفحة الرئيسية المباشرة:",
                ["These service cards are published on the homepage inside the Ecosystem Framework (Vision) section. The standalone AI & Engineering section is managed under Homepage Sections."] = "تُنشر بطاقات الخدمات هذه على الصفحة الرئيسية داخل قسم إطار عمل المنظومة (الرؤية). يُدار قسم الذكاء الاصطناعي والهندسة المستقل من خلال أقسام الصفحة الرئيسية.",
                ["Card Icon"] = "أيقونة البطاقة",
                ["Live Page Blocks: Five Pillars & Core Beliefs"] = "أقسام الصفحة المباشرة: خمس ركائز وما نؤمن به",
                ["Sections visibly published on the About Us page including Five Pillars, One Vision and What We Believe In cards."] = "الأقسام المنشورة على صفحة من نحن بما في ذلك بطاقات خمس ركائز ورؤية واحدة وما نؤمن به.",
                ["No live page blocks configured for the About page yet."] = "لا توجد أقسام صفحات مباشرة مضافة لصفحة من نحن بعد.",
                ["Core Beliefs"] = "المبادئ الأساسية",
                ["5 Divisions"] = "5 أقسام",
                ["Live Sections"] = "أقسام مباشرة",
                ["Edit Five Pillars, One Vision & What We Believe In"] = "تعديل خمس ركائز ورؤية واحدة وما نؤمن به",
                ["Five Pillars & Beliefs"] = "خمس ركائز والمبادئ",
                ["Content Page: About Us"] = "صفحة المحتوى: من نحن",
                ["What We Believe In (Principles) Band"] = "قسم ما نؤمن به (المبادئ)",
                ["Five Pillars, One Vision (Structure) Band"] = "قسم خمس ركائز، رؤية واحدة (الهيكل التنظيمي)",
                ["Principle cards"] = "بطاقات المبادئ",
                ["Subsidiary cards"] = "بطاقات الأقسام التابعة"
            };

            foreach (var kv in newEntries)
            {
                dict[kv.Key] = kv.Value;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            File.WriteAllText(arJsonPath, JsonSerializer.Serialize(dict, options));
            _output.WriteLine($"Saved cms.ar.json with {dict.Count} keys.");
        }
    }
}
