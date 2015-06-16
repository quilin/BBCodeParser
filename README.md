This library was written for my web-service [http://dungeonmaster.ru](http://dungeonmaster.ru), for it uses old-school forum-style, which requires BBCode.

There are plenty of fish in the sea, like [https://bbcode.codeplex.com](https://bbcode.codeplex.com), but it doesn't provide reverse parsing, which is necessary feature for me.
For the obvious reason that displaying posts (html version) is more often issue than editing it (bb version), it is necessary to keep text in your database in html format. As long as you need to let users update their posts, you need to be able to parse html back to bb.

The project is still in development: for example, i need to make it more safe, for right now it is possible to put html-tags inside the code.

How to use it?

    using BBCodeParser;

    public class HelloWorld
    {
        public static void Main(params string[] args)
        {
            var bbParser = new BBParser(new[] {
                                        new BBTag("b", "<strong>", "</strong>"),
                                        new BBTag("i", "<em>", "</em>"),
                                        new BBTag("link", "<a href=\"{url}\">", "</a>", new Dictionary<string, string>{{"url", "href"}}),
                                        new BBTag("img", "<img src=\"{url}\">", new Dictionary<string, string>{{"url", "src"}}),
                                        new BBTag("quote", "<div>{quote}</div><div>", "</div>", null, selfAttributed: true)
           });

           var html = bbParser.Parse("[b]hello, [i]world[/i][/b]", DirectionMode.BBToHtml); // <strong>hello, <em>world</em></strong>
           var bb = bbParser.Parse(html, DirectionMode.HtmlToBB); // [b]hello, [i]world[/i][/b]

           var htmlLink = bbParser.Parse("[link url=\"http://google.com\"]Google[/link]", DirectionMode.BBToHtml); // <a href="http://google.com">Google</a>
           var bbLink = bbParser.Parse(htmlLink, DirectionMode.HtmlToBB); // [link url="http://google.com"]Google[/link]

           var htmlImage = bbParser.Parse("[img url=\"myimage.jpg\"]", DirectionMode.BBToHtml); // <img src="myimage.jpg">
           var bbImage = bbParser.Parse(htmlImage, DirectionMode.HtmlToBB); //[img url="myimage.jpg"]

           var htmlQuote = bbParser.Parse("[quote=\"Vladimir Lenin\"]Learn, learn and once again - learn![/quote]"); // <div>Vladimir Lenin</div><div>Learn, learn and once again - learn!</div>
           var bbQuote = bbParser.Parse(htmlQuote, DirectionMode.HtmlToBB); // [quote="Vladimir Lenin"]Learn, learn and once again - learn![/quote]
        }
    }
