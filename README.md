This library was written for my web-service [http://dungeonmaster.ru](http://dungeonmaster.ru), for it uses old-school forum-style, which requires BBCode.

After long thinking I decided to come back to proper one-way parsing. Even with regexp pattern searching the parser shows satisfying results on large amount of nodes. You can see the ignored `PerformanceTests.cs` for more details. It is ignored because it can run up to 40 seconds =).

Because of those changes I was able to come up with more features and securities. E.g. as long as you no longer recieve the plain string as parsing result, but instead `NodeTree`, you can cast it to any format you need or use predefined `ToHtml`, `ToText` and `ToBb`. There are also more different tags and more substitutions options.

# How to use it?

## BbParser

    public BbParser(Tag[] tags, Dictionary<string, string> securitySubstitutions, Dictionary<string, string> aliasSubstitutions)

The BbParser is an entry point for the library, and you should use the only constructor for it. It takes three arguments, as you can see.

`tags` is an array of `Tag`, which declares what tags are allowed for this particular parser. You can create two different parsers with different security policies and different tag sets, it can be useful, if you want to give administrators and users different rights to display the content.
`securitySubstitutions` is a set of string pairs, that will substitute keys with values every time parser faces it. The default substitutions are `& => &amp;`, `< => &lt;` and `> => &gt;` and you can get it from a static field `BbParser.SecuritySubstitutions`.
`aliasSubstitutions` is a set of substitutions that has no security matter and used only for visual purposes. Like those: `-- => &ndash;`, `\t => &nbsp;&nbsp;` etc. These substitutions are only used after security checks, so you can use html in values.

## Tag

First of all, you need to define your tags. You can pick from 3 different types of tags:

### Simple Tag

Two constructors available for this tag:

    public Tag(string name, string openTag, string closeTag, bool withAttribute = false, bool secure = true)
    public Tag(string name, string openTag, bool withAttribute = false, bool secure = true)

First one is for a normal tag with open-and-close-pattern. The second one is for single tags, like image. First parameter (`name`) is the name of tag, that will be used inside bb-code. `openTag` stands for html-pattern of opening bb-tag, `closeTag` - for closing tag. `withAttribute` is a flag which defines whether the attribute is allowed for this tag, and `secure` is a flag which turns on the xss-protection for those attributed tags.

By using the second constructor, you shall create a tag without closing pattern, like image or iframe.

### Attributed tag

So far it is only allowed to have one attribute per tag. It is possible to create such attributed tag by passing `withAttribute: true` as an argument to constructor. Since that you can put in `openTag` or `closeTag` pattern substring `{value}` to render the attribute value from string such as `[tag="value"]`. You can also do some extra, which will be mentioned later.

### `PreformattedTag` and `CodeTag`

These are tags which allow you to escape some parser things. First one only escapes alias substitutions, second one also escapes all the bb-tags within. Same constructor for both:

    public PreformattedTag(string name, string openTag, string closeTag)
    public CodeTag(string name, string openTag, string closeTag)

## Example

    using BbCodeParser;

    public static class HelloWorld
    {

      public static void Main(params string[] args)
      {

        var bbParser = new BbParser(new [] {
          new Tag("b", "<strong>", "</strong>"),
          new Tag("tab", "&nbsp;&nbsp;&nbsp;"),
          new Tag("link", "<a href=\"{value}\">", "</a>", withAttribute: true, secure: true),
          new Tag("quote", "<div class=\"quote\">{value}:", "</div>", withAttribute: true, secure: false),
          new Tag("img", "<img src=\"{value}\" />", withAttribute: true, secure: true),
          new CodeTag("code", "<pre class=\"code\">", "</pre>"),
          new PreformattedTag("pre", "<pre class=\"code\">", "</pre>")
        }, BbParser.SecuritySubstitutions, new Dictionary<string, string> {
          {"--", "&ndash;"},
          {"---", "&mdash;"},
          {"\r\n___\r\n", "<hr />"},
          {"\r\n", "<br />"}
        });

      }

    }

# What next?

## `BbParser.Parse`

Now that you have the `BbParser` instance, you can parse something with it, by passing the string into `Parse` method.

    var nodeTree = bbParser.Parse("[b]Hello, world![/b]");

This method will return the `NodeTree` instance. There are three methods allowed for `NodeTree`:

 - `ToBb()` -- although it may sound stupid, but you can convert the text back to Bb. It is not required, but it will fix all possible user errors (e.g `[b]Wrong[i]Closing[/b]Order[/i]`) and will guarantee you a valid bb-code.
 - `ToText()` -- this one will return the text string, without any bb-codes. It will still replace `aliasSubstitution` though.
 - `ToHtml` -- this one requires a deeper explanation.

 ## `NodeTree.ToHtml`

    public string ToHtml(Func<Node, bool> filter = null, Func<Node, string, string> filterAttributeValue = null)

Although you may use it just like `ToBb`/`ToText`, sometimes you need to hide some information from some users. You know, those old forum policies that forbid guest user to see links or images that are embedded via bb. In my case I created special tag `[private="UserName1, UserName2"]` to send private messages right inside bb-code. It is very useful in forum-based RPGs.
`Filter` is pretty simple thing, you get the `TagNode` as an input and you should return whether to render this `Node` or not.

### `TagNode`

It consists of two fields: `Tag` (which is very same `Tag` that you use to define the parser) and `AttributeValue` for attributed tags. Both are readonly.

Sometimes you also want to modify the attribute values for some reasons (maybe you want to wrap outer links with your special fishing-detecting-system or replace same-domain links with relative paths). That's what you need `filterAttributeValue` for. You take `TagNode` and the attribute value and return new attribute value, that will be rendered.

## Example
```csharp
    using BbCodeParser;

    public static class HelloWorld
    {

      public static void Main(params string[] args)
      {

        var bbParser = new BbParser(new [] {
          new Tag("b", "<strong>", "</strong>"),
          new Tag("tab", "&nbsp;&nbsp;&nbsp;"),
          new Tag("link", "<a href=\"{value}\">", "</a>", withAttribute: true, secure: true),
          new Tag("quote", "<div class=\"quote\">{value}:", "</div>", withAttribute: true, secure: false),
          new Tag("img", "<img src=\"{value}\" />", withAttribute: true, secure: true),
          new CodeTag("code", "<pre class=\"code\">", "</pre>"),
          new PreformattedTag("pre", "<pre class=\"code\">", "</pre>")
        }, BbParser.SecuritySubstitutions, new Dictionary<string, string> {
          {"--", "&ndash;"},
          {"---", "&mdash;"},
          {"\r\n___\r\n", "<hr />"},
          {"\r\n", "<br />"}
        });

        var nodeTree = bbParser.Parse("[b]Hello, world![/b]");
        nodeTree.ToHtml(); // <strong>Hello, world!</strong>

        var linkNodeTree = bbParser.Parse("[link=\"Hi\"]Link[/link]")
        linkNodeTree.ToHtml(); // <a href="Hi">Link</a>
        linkNodeTree.ToText(); // Link

        var nodeTreeFromInvalidString = bbParser.Parse("[b][quote]Parser[/b]Can fix[/b]Things");
        nodeTreeFromInvalidString.ToBb(); // [b][quote]Parser[/quote][/b]Can fixThings
        nodeTreeFromInvalidString.ToHtml(); // <strong><div class="quote">Parser</div></strong>Can fixThings

        var codeTree = bbParser.Parse("[code][b]Hello --[/b][/code]");
        codeTree.ToHtml(); // <pre class="code">[b]Hello --[/b]</pre>

        var preTree = bbParser.Parse("[pre][b]Hello --[/b][/code]");
        preTree.ToHtml(); // <pre class="code"><strong>Hello --</strong></pre>

      }

    }```
