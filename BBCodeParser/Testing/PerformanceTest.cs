using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BBCodeParser;
using BBCodeParser.Tags;
using NUnit.Framework;

namespace Testing
{
	[TestFixture]
	public class PerformanceTest
	{
		private BbParser _bbCodeParser;
		private Tag[] _tags;
		private const string CodeClassName = "code";
		private const string SpoilerHeadClassName = "spoiler-head";
		private const string SpoilerClassName = "spoiler";
		private const string ImageClassName = "image";
		private const string QuoteClassName = "quote";

		[SetUp]
		public void SetUp()
		{
			_tags = new[]
			{
				new Tag("b", "<strong>", "</strong>"),
				new Tag("i", "<em>", "</em>"),
				new Tag("u", "<u>", "</u>"),
				new Tag("s", "<s>", "</s>"),
				new Tag("code", $"<pre class=\"{CodeClassName}\">", "</pre>"),
				new Tag("spoiler", $"<a href=\"javascript:void(0)\" class=\"{SpoilerHeadClassName}\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"{SpoilerClassName}\">", "</div>"), 
				new Tag("img", $"<a href=\"{{value}}\" target=\"_blank\"><img src=\"{{value}}\" class=\"{ImageClassName}\" /></a>", true),
				new Tag("link", "<a href=\"{value}\">", "</a>", true),
				new Tag("quote", $"<div class=\"{QuoteClassName}\">", "</div>"),
				new Tag("tab", "&nbsp;&nbsp;&nbsp;")
			};
			_bbCodeParser = new BbParser(_tags,
				BbParser.SecuritySubstitutions,
                new Dictionary<string, string>
                {
                    {"---", "&mdash;"},
                    {"--", "&ndash;"},
                    {"\r\n", "<br />"}
                });
		}

		[Test]
		public void Test1()
		{
			Assert.IsTrue(true);
		}

		[Test]
		[TestCase(1, 200)]
		[TestCase(10, 2000)]
		[TestCase(50, 10000)]
		[TestCase(200, 40000)]
		public void TestManyTags(int textCount, int expectedPerformance)
		{
			var inputs = new List<string>();
			for (var j = 0; j < textCount; j++)
			{
				var inputBuilder = new StringBuilder();
				for (var i = 0; i < 20000; i++)
				{
					var tag = _tags[i % _tags.Length];
					inputBuilder.Append(string.Format("[{0}{1}]{2}",
						tag.Name,
						tag.WithAttribute ? "=\"test\"" : string.Empty,
						tag.RequiresClosing ? string.Format("{0}[/{1}]", "test", tag.Name) : string.Empty));
				}
				inputs.Add(inputBuilder.ToString());
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			foreach (var input in inputs)
			{
				var actual = _bbCodeParser.Parse(input);
				actual.ToHtml();
			}
			stopwatch.Stop();

			Assert.Pass("Elapsed: {0}", stopwatch.ElapsedMilliseconds);
			Assert.Less(stopwatch.ElapsedMilliseconds, expectedPerformance);
		}

		[Test]
		[TestCase(100, 2)]
		[TestCase(500, 250)]
		[TestCase(1000, 100)]
		[TestCase(2000, 500)]
		[TestCase(4000, 2200)]
		[TestCase(5000, 10000)]
		public void TestManyInnerTags(int tagsCount, int expectedPerformance)
		{
			var tags = _tags.Where(t => t.RequiresClosing).ToArray();
			var inputBuilder = new StringBuilder();
			for (var i = 0; i < tagsCount; ++i)
			{
				var tag = tags[i%tags.Length];
				inputBuilder.Append(string.Format("[{0}{1}]lalala", tag.Name, tag.WithAttribute ? "=\"test\"" : string.Empty));
			}
			for (var i = tagsCount - 1; i >= 0; --i)
			{
				var tag = tags[i%tags.Length];
				inputBuilder.Append(string.Format("[/{0}]", tag.Name));
			}
			var input = inputBuilder.ToString();

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var actual = _bbCodeParser.Parse(input);
			actual.ToHtml();
			stopwatch.Stop();

			Assert.Pass("Elapsed: {0}", stopwatch.ElapsedMilliseconds);
			Assert.Less(stopwatch.ElapsedMilliseconds, expectedPerformance);
		}
	}
}