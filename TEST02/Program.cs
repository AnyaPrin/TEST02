using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic; // Dictionary を使用するために追加

namespace TEST02;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        System.Diagnostics.Debug.WriteLine("DEBUG: Main method started."); // この行を追加

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddCommandLine(args, new Dictionary<string, string> // コマンドライン引数を追加
            {
                { "-g", "AppSettings:ImageFileName" },
                { "--image", "AppSettings:ImageFileName" },
                { "-f", "AppSettings:CatResponsesFileName" },
                { "--responses", "AppSettings:CatResponsesFileName" }
            })
            .Build();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1(configuration)); // configuration を Form1 に渡す

        System.Diagnostics.Debug.WriteLine("DEBUG: Main method completed."); // この行を追加
        return 0; // 正常終了を示すために0を返す

    }    
}