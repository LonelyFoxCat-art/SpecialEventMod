namespace VersatileSpecialEventAPI.Utils;

/// <summary>
/// 提供文件与目录基础操作的静态辅助方法。
/// </summary>
public static class FileHelpr {
    /// <summary>
    /// 检查指定的路径是否对应一个已存在的文件或目录。
    /// </summary>
    /// <param name="FilePath">要检查的文件或目录的绝对/相对路径。</param>
    /// <returns>如果文件或目录存在，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public static bool IsExist(string FilePath) => new DirectoryInfo(FilePath).Exists || new FileInfo(FilePath).Exists;

    /// <summary>
    /// 检查指定的 <see cref="FileInfo"/> 对象所代表的文件是否存在。
    /// </summary>
    /// <param name="File">要检查的文件信息对象。</param>
    /// <returns>如果文件存在，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public static bool IsExist(FileInfo File) => File.Exists;

    /// <summary>
    /// 检查指定的 <see cref="DirectoryInfo"/> 对象所代表的目录是否存在。
    /// </summary>
    /// <param name="Directory">要检查的目录信息对象。</param>
    /// <returns>如果目录存在，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public static bool IsExist(DirectoryInfo Directory) => Directory.Exists;

    /// <summary>
    /// 获取指定目录中的所有文件。
    /// </summary>
    /// <param name="Directory">目标目录信息对象。</param>
    /// <returns>包含当前目录中文件的 <see cref="FileInfo"/> 数组。</returns>
    public static FileInfo[] GetFiles(DirectoryInfo Directory) => Directory.GetFiles();

    /// <summary>
    /// 获取指定路径下匹配搜索模式的文件列表。
    /// </summary>
    /// <param name="FilePath">要搜索的目录的相对或绝对路径。</param>
    /// <param name="Pattern">搜索字符串（如 "*.txt"）。默认为 "*"（所有文件）。</param>
    /// <param name="Option">指定搜索操作是应包含所有子目录还是仅包含当前目录。默认为 <see cref="SearchOption.TopDirectoryOnly"/>。</param>
    /// <returns>匹配指定搜索模式的文件的 <see cref="FileInfo"/> 数组。</returns>
    /// <exception cref="DirectoryNotFoundException">指定的路径无效（例如，它位于未映射的驱动器上）。</exception>
    public static FileInfo[] GetFiles(string FilePath, string Pattern = "*", SearchOption Option = SearchOption.TopDirectoryOnly) => new DirectoryInfo(FilePath).GetFiles(Pattern, Option);

    /// <summary>
    /// 获取指定目录中的子目录。
    /// </summary>
    /// <param name="Directory">目标目录信息对象。</param>
    /// <returns>包含当前目录中子目录的 <see cref="DirectoryInfo"/> 数组。</returns>
    public static DirectoryInfo[] GetDirectories(DirectoryInfo Directory) => Directory.GetDirectories();

    /// <summary>
    /// 获取指定路径中的子目录。
    /// </summary>
    /// <param name="Path">要搜索的目录的相对或绝对路径。</param>
    /// <returns>包含指定路径中子目录的 <see cref="DirectoryInfo"/> 数组。</returns>
    public static DirectoryInfo[] GetDirectories(string Path) => new DirectoryInfo(Path).GetDirectories();

    /// <summary>
    /// 获取指定路径文件的扩展名（包含前导点，如 ".txt"）。
    /// </summary>
    /// <param name="FilePath">文件路径。</param>
    /// <returns>文件的扩展名；如果文件没有扩展名或路径为 null，则返回 <see cref="string.Empty"/>。</returns>
    public static string GetExtension(string FilePath) => new FileInfo(FilePath).Extension;

    /// <summary>
    /// 获取指定 <see cref="FileInfo"/> 对象的扩展名。
    /// </summary>
    /// <param name="File">文件信息对象。</param>
    /// <returns>文件的扩展名。</returns>
    public static string GetExtension(FileInfo File) => File.Extension;

    /// <summary>
    /// 获取指定路径文件的不含扩展名的名称。
    /// </summary>
    /// <param name="FilePath">文件路径。</param>
    /// <returns>不含扩展名的文件名。</returns>
    /// <remarks>
    public static string GetName(string FilePath) => Path.GetFileNameWithoutExtension(new FileInfo(FilePath).Name);

    /// <summary>
    /// 获取指定 <see cref="FileInfo"/> 对象的不含扩展名的名称。
    /// </summary>
    /// <param name="File">文件信息对象。</param>
    /// <returns>不含扩展名的文件名。</returns>
    public static string GetName(FileInfo File) => Path.GetFileNameWithoutExtension(File.Name);
}

