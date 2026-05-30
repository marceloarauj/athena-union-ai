using System.Text;

namespace AthenaUnionAI.Infrastructure.Services
{
    public record MarkdownChunk
    (
        string Content,
        string FileName,
        string Section,
        string HeadingPath
    );

    public class MarkdownChunkingService
    {
        public IEnumerable<MarkdownChunk> ChunkDirectory(string docsRoot)
        {
            foreach (var filePath in Directory.EnumerateFiles(docsRoot, "*.md", SearchOption.AllDirectories))
            {
                foreach (var chunk in ChunkFile(filePath, docsRoot))
                    yield return chunk;
            }
        }

        private static IEnumerable<MarkdownChunk> ChunkFile(string filePath, string docsRoot)
        {
            var raw = File.ReadAllText(filePath);
            var relativePath = Path.GetRelativePath(docsRoot, filePath).Replace('\\', '/');
            var content = StripFrontmatter(raw).Trim();

            if (string.IsNullOrWhiteSpace(content))
                yield break;

            var title = ExtractH1(content);
            var sections = ExtractH2Sections(content);

            if (sections.Count == 0)
            {
                yield return new MarkdownChunk(content, relativePath, string.Empty, title);
                yield break;
            }

            foreach (var (heading, body) in sections)
            {
                var chunkContent = new StringBuilder();
                if (!string.IsNullOrEmpty(title))
                    chunkContent.AppendLine($"# {title}").AppendLine();
                chunkContent.AppendLine($"## {heading}").AppendLine();
                chunkContent.Append(body.Trim());

                var headingPath = string.IsNullOrEmpty(title) ? heading : $"{title} > {heading}";

                yield return new MarkdownChunk(
                    chunkContent.ToString().Trim(),
                    relativePath,
                    heading,
                    headingPath
                );
            }
        }

        private static string StripFrontmatter(string content)
        {
            if (!content.TrimStart().StartsWith("---"))
                return content;

            var start = content.IndexOf("---");
            var end = content.IndexOf("---", start + 3);

            return end < 0 ? content : content[(end + 3)..].TrimStart('\n', '\r');
        }

        private static string ExtractH1(string content)
        {
            foreach (var line in content.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("# ") && !trimmed.StartsWith("## "))
                    return trimmed[2..].Trim();
            }
            return string.Empty;
        }

        private static List<(string Heading, string Body)> ExtractH2Sections(string content)
        {
            var result = new List<(string, string)>();
            var lines = content.Split('\n');

            string? currentHeading = null;
            var currentBody = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.StartsWith("## "))
                {
                    if (currentHeading is not null)
                        result.Add((currentHeading, currentBody.ToString().Trim()));

                    currentHeading = line[3..].Trim();
                    currentBody.Clear();
                }
                else if (currentHeading is not null)
                {
                    currentBody.AppendLine(line);
                }
            }

            if (currentHeading is not null)
                result.Add((currentHeading, currentBody.ToString().Trim()));

            return result;
        }
    }
}
