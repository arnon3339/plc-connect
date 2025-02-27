using Tomlyn;
using Tomlyn.Model;

class Config
{
   public static TomlTable config;

    static Config()
    {
        var filePath = "config.toml";
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            config = Toml.ToModel(content);
        }
        else
        {
            throw new FileNotFoundException("Connot find config.toml file");
        }
    }
}