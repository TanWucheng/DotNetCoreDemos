using System.Text;

namespace ConsoleDemo.StringComparison
{
    /// <summary>
    /// 获取两个字串按顺序的最长交集
    /// </summary>
    internal class StringSamePart
    {
        public static string Get(string str1, string str2)
        {
            int[,] num = new int[str1.Length, str2.Length];
            int maxLen = 0; int lastSubsBegin = 0;
            StringBuilder sequenceBuilder = new StringBuilder();
            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                        num[i, j] = 0;
                    else
                    {
                        if (i == 0 || j == 0)
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxLen)
                        {
                            maxLen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {
                                // If the current LCS is the same as the last time this block ran
                                sequenceBuilder.Append(str1[i]);
                            }
                            else
                            {
                                // Reset the string builder if a different LCS is found
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0;
                                sequenceBuilder.Append(str1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }
            return sequenceBuilder.ToString();
            //var str = $"The longest common substring of '{str1}' and '{str2}' is '{sequenceBuilder.ToString()}'.";
        }
    }
}