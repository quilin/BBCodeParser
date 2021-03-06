﻿using System;
using System.Collections.Generic;
using System.Linq;
using BBCodeParser;
using BBCodeParser.Nodes;
using BBCodeParser.Tags;
using NUnit.Framework;

namespace Testing
{
    [TestFixture]
    public class TestParse
    {
        private BbParser bbCodeParser;
        private const string CodeClassName = "code";
        private const string PreClassName = "pref";
        private const string SpoilerHeadClassName = "spoiler-head";
        private const string SpoilerClassName = "spoiler";
        private const string ImageClassName = "image";
        private const string QuoteClassName = "quote";
        private const string QuoteAuthorClassName = "quote-author";

        [SetUp]
        public void SetUp()
        {
            bbCodeParser = new BbParser(new[]
                {
                    new Tag("b", "<strong>", "</strong>"),
                    new Tag("i", "<em>", "</em>"),
                    new Tag("u", "<u>", "</u>"),
                    new Tag("s", "<s>", "</s>"),
                    new PreformattedTag("pre", $"<pre class=\"{PreClassName}\">", "</pre>"),
                    new Tag("spoiler",
                        $"<a href=\"javascript:void(0)\" class=\"{SpoilerHeadClassName}\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"{SpoilerClassName}\">",
                        "</div>"),
                    new Tag("img",
                        $"<a href=\"{{value}}\" target=\"_blank\"><img src=\"{{value}}\" class=\"{ImageClassName}\" /></a>",
                        true),
                    new Tag("link", "<a href=\"{value}\">", "</a>", true),
                    new Tag("quote",
                        $"<div class=\"{QuoteClassName}\"><div class=\"{QuoteAuthorClassName}\">{{value}}</div>",
                        "</div>", true, false),
                    new Tag("tab", "&nbsp;&nbsp;&nbsp;"),
                    new Tag("private", "{value}", "", true, false),
                    new CodeTag("code", $"<pre class=\"{CodeClassName}\">", "</pre>"),
                    new ListTag("ul", "<ul>", "</ul>"),
                    new Tag("li", "<li>", "</li>")
                },
                BbParser.SecuritySubstitutions,
                new Dictionary<string, string>
                {
                    {"---", "&mdash;"},
                    {"--", "&ndash;"},
                    {"\r\n", "<br />"}
                });
        }

        [Test]
        public void TestParseSimple()
        {
            var input = "[b]hello[/b]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<strong>hello</strong>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestParseWithMoreClosingTags()
        {
            var actual = bbCodeParser.Parse("[b]hello[/b][/b][/b]");
            Assert.AreEqual("<strong>hello</strong>", actual.ToHtml());
        }

        [Test]
        public void TestParseSimpleLinear()
        {
            var actual = bbCodeParser.Parse("[b]hello[/b] [i]world[/i]");
            Assert.AreEqual("<strong>hello</strong> <em>world</em>", actual.ToHtml());
        }

        [Test]
        public void TestParseSimpleTree()
        {
            var actual = bbCodeParser.Parse("[b]hello [i]world[/i][/b]");
            Assert.AreEqual("<strong>hello <em>world</em></strong>", actual.ToHtml());
        }

        [Test]
        public void TestParseInvalidTree()
        {
            var actual1 = bbCodeParser.Parse("[b]hello [i]world[/b][/i]");
            var actual2 = bbCodeParser.Parse("[b]hello [i]world[/i][/i]");
            var actual3 = bbCodeParser.Parse("[b]hello [i]world[/b][/b]");
            var expected = "<strong>hello <em>world</em></strong>";
            Assert.AreEqual(expected, actual1.ToHtml());
            Assert.AreEqual(expected, actual2.ToHtml());
            Assert.AreEqual(expected, actual3.ToHtml());
        }

        [Test]
        public void TestLink()
        {
            var input = "[link=\"hello\"]linktext[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"hello\">linktext</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestImage()
        {
            var input = "[img=\"hello\"]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"hello\" target=\"_blank\"><img src=\"hello\" class=\"image\" /></a>",
                actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestTab()
        {
            var input = "hello[tab]bye";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("hello&nbsp;&nbsp;&nbsp;bye", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestFail()
        {
            var input = "[[b]aaa[/b]]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("[<strong>aaa</strong>]", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestSpoiler()
        {
            var input = "[link=\"http://yandex.ru\"]Яндекс[/link][spoiler]hehehe[/spoiler]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual(
                "<a href=\"http://yandex.ru\">Яндекс</a><a href=\"javascript:void(0)\" class=\"spoiler-head\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"spoiler\">hehehe</div>",
                actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestOptionalAttribute()
        {
            var input1 = "[quote=\"Author\"]test[/quote]";
            var actual1 = bbCodeParser.Parse(input1);
            Assert.AreEqual("<div class=\"quote\"><div class=\"quote-author\">Author</div>test</div>",
                actual1.ToHtml());
            Assert.AreEqual(input1, actual1.ToBb());

            var input2 = "[quote]test[/quote]";
            var actual2 = bbCodeParser.Parse(input2);
            Assert.AreEqual("<div class=\"quote\"><div class=\"quote-author\"></div>test</div>", actual2.ToHtml());
            Assert.AreEqual(input2, actual2.ToBb());
        }

        [Test]
        public void TestWithAttribute()
        {
            var input = "[spoiler][img=\"https://imgurl.jpg\"][/spoiler]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual(
                "<a href=\"javascript:void(0)\" class=\"spoiler-head\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"spoiler\"><a href=\"https://imgurl.jpg\" target=\"_blank\"><img src=\"https://imgurl.jpg\" class=\"image\" /></a></div>",
                actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestCode()
        {
            var input =
                "[b]Not inside the code[/b]... [i]not yet[/i]. [code]And [b]this one is[/b]<script></script>[/code]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual(
                "<strong>Not inside the code</strong>... <em>not yet</em>. <pre class=\"code\">And [b]this one is[/b]&lt;script&gt;&lt;/script&gt;</pre>",
                actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestJsInjection()
        {
            var input = "[link=\"http://yandex.ru\' onload='alert\"]coolhack[/link]\"";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"http://yandex.ruonload=alert\">coolhack</a>\"", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestHtmlInjection()
        {
            var input = "[b]<script>console.log('hi');</script>&nbsp;[/b]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<strong>&lt;script&gt;console.log('hi');&lt;/script&gt;&amp;nbsp;</strong>",
                actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestBugWithTextAfterEndingTag()
        {
            var input = "test![b]Test [link=\"url\"]link[/link]text[/b]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("test!<strong>Test <a href=\"url\">link</a>text</strong>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestJsInjectionWithExecuting()
        {
            var input = "[link=\"Javascript:alert(`XSS`)\"]alert[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"_xss_alert(XSS)\">alert</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestBugWithInvalidTree()
        {
            var input = "test![b]Test[i]1[/b]test";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("test!<strong>Test<em>1</em></strong>test", actual.ToHtml());
            Assert.AreEqual("test![b]Test[i]1[/i][/b]test", actual.ToBb());
        }

        [Test]
        public void TestBugWithHiddenXss()
        {
            var input = "[link=\"JaVas\"C'ript:alert(document.cookie)\"]aaa[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"_xss_alert(document.cookie)\">aaa</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestXss1()
        {
            var input = "[link=\"javascript&'#058;alert(/xss/)\"]ddd[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"javascriptalert(/xss/)\">ddd</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestXss2()
        {
            var input = "[link=\"Javas&'#x09;cript:alert(/xss/)\"]123[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"_xss_alert(/xss/)\">123</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestXss3()
        {
            var input =
                "[link=\"javascriptJa vA s\"'\"'    \"'''\"\"\"\"   cript::alert(\"/Preved, admincheg/\")\"]Preved[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"javascript_xss_:alert(/Preved,admincheg/)\">Preved</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestXss4()
        {
            var input = "[link=\"javasc&\"#0000009ript:alert(/xss/)\"]ddd[/link]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual("<a href=\"javasc:alert(/xss/)\">ddd</a>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestCodeResolvingInnerBbIssues()
        {
            var input = "[code][i]test[/code]";
            var actual = bbCodeParser.Parse(input);

            Assert.AreEqual("<pre class=\"code\">[i]test</pre>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestNonTaggedInputsBug()
        {
            var input = @"[notaspoi...""
test [s]test[/s]";
            var actual = bbCodeParser.Parse(input);

            Assert.AreEqual("[notaspoi...\"<br />test <s>test</s>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestNullReferenceFallbackBug1()
        {
            var input =
                @"Порядок тегов имеет значение при использовании тега head (проверял в [link=""http://dungeonmaster.ru.trioptimum.ru/profile/Hatchet""]своем профиле[/link])\r\n\r\n1. [code][head][b]Lorem Ipsum[/b][/head][/code] - на выходе получаем полужирный заголовок\r\n2. [code][b][head]Lorem Ipsum[/head][/b][/code] - на выходе получаем просто заголовок\r\nВ то же время для остальных тегов - b, i, s, u - порядок вложения значения не имеет.\r\n\r\nОжидание: оба варианта выше должны работать одинаково.\r\n\r\nP.S. А в [link=""http://dungeonmaster.ru.trioptimum.ru/parsertest""]тесте парсера[/link] тег head вообще не работает";
            NodeTree actual = null;
            Assert.DoesNotThrow(delegate { actual = bbCodeParser.Parse(input); });
            Assert.DoesNotThrow(delegate { actual.ToHtml(); });
            Assert.DoesNotThrow(delegate { actual.ToBb(); });
            Assert.DoesNotThrow(delegate { actual.ToText(); });
        }

        [Test]
        public void TestNullReferenceFallbackBug2()
        {
            var input =
                "[spoiler][img=\"http://s05.radikal.ru/i178/1609/96/a9db8fef8310.png\"][/spoiler][spoiler][img=\"http://s019.radikal.ru/i614/1609/e0/0cf83e802ef7.png\"][/spoiler]\r\nКогда ставятся теги спойлера и цитаты, они отображаются на тексте с лишней пустой строкой, если писать текст начинать не сразу же после тега, а в другой строке.\r\n[code][spoiler]Спойлер[/spoiler]текст сразу после тега[/code] преобразится в \r\n\"скрыть содержимое\r\nтекст сразу после тега\"\r\n\r\nТо же самое с цитатой. \r\n\r\nНо если сделать:\r\n[code][spoiler]Спойлер[/spoiler]\r\nТекст в другой строке после тега[/code]\r\nоно преобразится в \r\n\"показать содержимое\r\n\r\nТекст в другой строке после тега\"\r\nТ.е., появится пустая строка. Это очень неудобно при форматировании текста, приходится постоянно учитывать этот момент, что появится пустая строка. А если хочешь сделать цепочку без разрывов:\r\n\"текст\r\nспойлер\r\nтекст\r\nспойлер и тд\"\r\nТо её нужно записать сплошным текстом:  \"текст[спойлер]текст[спойлер]текст[спо...\", что опять же неудобно.\r\n\r\n[i]Предложение[/i]: не вставлять эту пустую строку у тегов спойлер и цитата, если текст после тега начинается в другой/других строке/строках.\r\n\r\nТ.е.\r\n\"[code][spoiler]Спойлер[/spoiler]текст сразу после тега[/code] \"\r\nпреобразится в \r\n\"скрыть содержимое\r\nтекст сразу после тега\"\r\n\r\n\"[code][spoiler]Спойлер[/spoiler]\r\nтекст в другой строке после тега[/code] \"\r\nпреобразится в \r\n\"скрыть содержимое\r\nтекст сразу после тега\"\r\n\r\n\"[code][spoiler]Спойлер[/spoiler]\r\n\r\nтекст во второй строке после тега[/code] \"\r\nпреобразится в\r\n\"скрыть содержимое\r\n\r\nтекст во второй строке после тега\"\r\n\r\nUPD: почему-то не хочет \"предложение\" выделяться курсивом. Делаю его [code][i]Предложение[/i]:[/code], а сохраняется \"[code][_i]Предложение: ([_/i] вот это тут само вставляется в конце, если убрать подчёркивание и оставить один [_i])[/code]\"";
            NodeTree actual = null;
            Assert.DoesNotThrow(delegate { actual = bbCodeParser.Parse(input); });
            Assert.DoesNotThrow(delegate { actual.ToHtml(); });
            Assert.DoesNotThrow(delegate { actual.ToBb(); });
            Assert.DoesNotThrow(delegate { actual.ToText(); });
        }

        [Test]
        public void TestBugWithDoublePrivate()
        {
            var input = "[code][private=\"Test\"]Test[/private][/code]";
            var actual = bbCodeParser.Parse(input);
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestFilter()
        {
            var input1 = "[private=\"Test1, Test2\"]Hi[/private]";
            var input2 = "[private=\"Test1, Test3\"]Hi[/private]";
            var input3 = "[private=\"Test1, Test2, Test3\"]Hi[/private]";
            var actual1 = bbCodeParser.Parse(input1);
            var actual2 = bbCodeParser.Parse(input2);
            var actual3 = bbCodeParser.Parse(input3);

            Func<Node, bool> filter = n => !(n is TagNode) || ((TagNode) n).AttributeValue.Split(',')
                                           .Select(v => v.Trim()).Any(v =>
                                               v.Equals("Test3", StringComparison.InvariantCultureIgnoreCase));
            Func<Node, string, string> filterAttributeValue = (v, n) => "Test3";

            Assert.IsEmpty(actual1.ToHtml(filter, filterAttributeValue));

            Assert.AreEqual("Test3Hi", actual2.ToHtml(filter, filterAttributeValue));
            Assert.AreEqual("Test1, Test3Hi", actual2.ToHtml(filter));

            Assert.AreEqual("Test3Hi", actual3.ToHtml(filter, filterAttributeValue));
            Assert.AreEqual("Test1, Test2, Test3Hi", actual3.ToHtml(filter));
        }

        [Test]
        public void TestAliasSubstitutions()
        {
            var input1 = "Hello< --[code] [b]&world --->[/code]";
            var input2 = "Hello< --[pre] [b]&world --->[/pre]";
            var actual1 = bbCodeParser.Parse(input1);
            var actual2 = bbCodeParser.Parse(input2);

            Assert.AreEqual("Hello&lt; &ndash;<pre class=\"code\"> [b]&amp;world ---&gt;</pre>", actual1.ToHtml());
            Assert.AreEqual(input1, actual1.ToBb());

            Assert.AreEqual("Hello&lt; &ndash;<pre class=\"pref\"> <strong>&amp;world ---&gt;</strong></pre>",
                actual2.ToHtml());
            Assert.AreEqual("Hello< --[pre] [b]&world --->[/b][/pre]", actual2.ToBb());
        }

        [Test]
        public void TestToBbAndTextFilters()
        {
            var input = "[i]Hello[/i] [b]world[/b]";
            var actual = bbCodeParser.Parse(input);

            Func<Node, bool> filter = n => !(n is TagNode) || ((TagNode) n).Tag.Name != "b";
            Assert.AreEqual("[i]Hello[/i] ", actual.ToBb(filter));
            Assert.AreEqual("Hello ", actual.ToText(filter));
        }

        [Test]
        public void TestToBbAndTextFiltersWithCode()
        {
            var input = "[i]Hello[/i] [code][b]world[/b][/code]";
            var actual = bbCodeParser.Parse(input);

            bool Filter(Node n) => !(n is TagNode) || ((TagNode) n).Tag.Name != "b";
            Assert.AreEqual("[i]Hello[/i] [code][b]world[/b][/code]", actual.ToBb(Filter));
            Assert.AreEqual("Hello [b]world[/b]", actual.ToText(Filter));
        }

        [Test]
        public void TestListCode()
        {
            var input = @"[ul]
[li]test1[/li]
[li]test2[/li][/ul]";
            var actual = bbCodeParser.Parse(input);

            Assert.AreEqual("<ul><li>test1</li><li>test2</li></ul>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestListCode2()
        {
            var input = @"[ul][li]чи шо[/li]
asdfas
[li][/li][/ul]";
            var actual = bbCodeParser.Parse(input);

            Assert.AreEqual("<ul><li>чи шо</li><li></li></ul>", actual.ToHtml());
            Assert.AreEqual(input, actual.ToBb());
        }

        [Test]
        public void TestRecursiveXss()
        {
            var input = "[link=\"jav&#&#90;90;ascript:\"]test[/link]";
            var actual = bbCodeParser.Parse(input);

            Assert.AreEqual("<a href=\"_xss_\">test</a>", actual.ToHtml());
        }

        [Test]
        public void TestHtmlXssInAttributes()
        {
            var input = "[quote=\"<script>alert('xss');</script>\"]test[/quote]";
            var actual = bbCodeParser.Parse(input);

            Assert.AreEqual("<div class=\"quote\"><div class=\"quote-author\">&lt;script&gt;alert('xss');&lt;/script&gt;</div>test</div>", actual.ToHtml());
        }
    }
}