using System.Collections.Generic;
using BBCodeParser;
using NUnit.Framework;

namespace Testing
{
    [TestFixture]
    public class TestParse
    {
        private BBParser bbCodeParser;
        private const string CodeClassName = "code";
        private const string SpoilerHeadClassName = "spoiler-head";
        private const string SpoilerClassName = "spoiler";
        private const string ImageClassName = "image";
        private const string QuoteClassName = "quote";

        [SetUp]
        public void SetUp()
        {
            bbCodeParser = new BBParser(new[]
            {
                new BBTag("b", "<strong>", "</strong>"),
                new BBTag("i", "<em>", "</em>"),
                new BBTag("u", "<u>", "</u>"),
                new BBTag("s", "<s>", "</s>"),
                new BBTag("code", string.Format("<pre class=\"{0}\">", CodeClassName), "</pre>"),
                new BBTag("spoiler", string.Format("<a href=\"#\" class=\"{0}\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"{1}\">", SpoilerHeadClassName, SpoilerClassName), "</div>"), 
                new BBTag("img", "<a href=\"{url}\" target=\"_blank\"><img src=\"{url}\" class=\"" + ImageClassName + "\" /></a>", new Dictionary<string, string>{{"url", "src"}}),
                new BBTag("link", "<a href=\"{url}\">", "</a>", new Dictionary<string, string>{{"url", "href"}}),
                new BBTag("quote", string.Format("<div class=\"{0}\">", QuoteClassName), "</div>"),
                new BBTag("tab", "&nbsp;&nbsp;&nbsp;"),
                new BBTag("private", "<test t=\"{private}\">", "</test>", null, true)
            },
                new Dictionary<string, string>
                {
                    {"---", "&mdash;"},
                    {"--", "&ndash;"},
                    {"<", "&lt;"},
                    {">", "&gt;"},
                    {"\r\n", "<br />"}
                });
        }

        [Test]
        public void TestSelfAttribute()
        {
            var input = "[private=\"Hi\"]test[/private]";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);
            Assert.AreEqual("<test t=\"Hi\">test</test>", actual);
            Assert.AreEqual(bbCodeParser.Parse(actual, DirectionMode.HtmlToBB), input);
        }

        [Test]
        public void TestParseSimple()
        {
            var input = "[b]hello[/b]";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);
            var actual1 = bbCodeParser.Parse(actual, DirectionMode.HtmlToBB);
            Assert.AreEqual("<strong>hello</strong>", actual);
            Assert.AreEqual(actual1, input);
        }

        [Test]
        public void TestParseWithMoreClosingTags()
        {
            var actual = bbCodeParser.Parse("[b]hello[/b][/b][/b]", DirectionMode.BBToHtml);
            Assert.AreEqual("<strong>hello</strong>", actual);
        }

        [Test]
        public void TestParseSimpleLinear()
        {
            var actual = bbCodeParser.Parse("[b]hello[/b] [i]world[/i]", DirectionMode.BBToHtml);
            Assert.AreEqual("<strong>hello</strong> <em>world</em>", actual);
        }

        [Test]
        public void TestParseSimpleTree()
        {
            var actual = bbCodeParser.Parse("[b]hello [i]world[/i][/b]", DirectionMode.BBToHtml);
            Assert.AreEqual("<strong>hello <em>world</em></strong>", actual);
        }

        [Test]
        public void TestParseInvalidTree()
        {
            var actual1 = bbCodeParser.Parse("[b]hello [i]world[/b][/i]", DirectionMode.BBToHtml);
            var actual2 = bbCodeParser.Parse("[b]hello [i]world[/i][/i]", DirectionMode.BBToHtml);
            var actual3 = bbCodeParser.Parse("[b]hello [i]world[/b][/b]", DirectionMode.BBToHtml);
            Assert.AreEqual("<strong>hello <em>world</em></strong>", actual1);
            Assert.AreEqual("<strong>hello <em>world</em></strong>", actual2);
            Assert.AreEqual("<strong>hello <em>world</em></strong>", actual3);
        }

        [Test]
        public void TestLink()
        {
            var input = "[link url=\"hello\"]linktext[/link]";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);
            Assert.AreEqual("<a href=\"hello\">linktext</a>", actual);
            actual = bbCodeParser.Parse(actual, DirectionMode.HtmlToBB);
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void TestImage()
        {
            var input = "[img url=\"hello\"]";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);
            Assert.AreEqual("<a href=\"hello\" target=\"_blank\"><img src=\"hello\" class=\"image\" /></a>", actual);
            actual = bbCodeParser.Parse(actual, DirectionMode.HtmlToBB);
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void TestTab()
        {
            var input = "hello[tab]bye";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);
            Assert.AreEqual("hello&nbsp;&nbsp;&nbsp;bye", actual);
            actual = bbCodeParser.Parse(actual, DirectionMode.HtmlToBB);
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void TestSpoiler()
        {
            var input = "[link url=\"http://yandex.ru\"]Яндекс[/link][spoiler]hehehe[/spoiler]";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);
            Assert.AreEqual("<a href=\"http://yandex.ru\">Яндекс</a><a href=\"#\" class=\"spoiler-head\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"spoiler\">hehehe</div>", actual);
            actual = bbCodeParser.Parse(actual, DirectionMode.HtmlToBB);
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void TestJsInjection()
        {
            var input = "[link url=\"http://yandex.ru\' onload='alert\"]coolhack[/link]";
            var actual = bbCodeParser.Parse(input, DirectionMode.BBToHtml);

            actual = bbCodeParser.Parse(actual, DirectionMode.HtmlToBB);
            Assert.AreEqual(input, actual);
        }
    }
}