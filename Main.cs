using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wox.Plugin;

namespace Wox.Plugin.XShell 

{
    public class Main : IPlugin
    {
        private PluginInitContext _context;
        private string _PATH_XSHELL_EXE;
        private string _PATH_XSHELL_SESSIONS_FOLDER;
        private bool m_bValidPath = false;
        public void Init(PluginInitContext context)
        {
            _context = context;

            Config cfg = new Config();
            string strErr = "";
            if(Config.Load(_context.CurrentPluginMetadata.PluginDirectory + "\\config.json", ref cfg, ref strErr))
            {
                _PATH_XSHELL_EXE = cfg.XShellExePath;
                _PATH_XSHELL_SESSIONS_FOLDER = cfg.XShellSessionsFolder;
                if(File.Exists(_PATH_XSHELL_EXE) && Directory.Exists(_PATH_XSHELL_SESSIONS_FOLDER))
                {
                    m_bValidPath = true;
                }
            }
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();

            if (!m_bValidPath)
            {
                results.Add(new Result()
                {
                    Title = "xsh",
                    SubTitle = "配置不正确，请安装xshell并正确配置config.json",
                    IcoPath = "app.png"
                });
                return results;
            }


            string[] strFilters = query.SecondToEndSearch.Split();
            string[] strXshList = GetXshList();
            string[] strFilteredXshList = FilterStringList(strXshList, strFilters);
            foreach(string xsh in strFilteredXshList)
            {
                results.Add(new Result()
                {
                    Title = xsh,
                    //SubTitle = "xshell",
                    IcoPath = "app.png",  //相对于插件目录的相对路径
                    Action = e =>
                    {
                        // 处理用户选择之后的操作
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = _PATH_XSHELL_EXE,
                                UseShellExecute = true,
                                Arguments = _PATH_XSHELL_SESSIONS_FOLDER + "\\" + xsh + ".xsh"
                            });
                        }
                        catch (Win32Exception)
                        {
                            var name = $"Plugin: {_context.CurrentPluginMetadata.Name}";
                            var message = "Can't open this file";
                            _context.API.ShowMsg(name, message, string.Empty);
                        }
                        _context.API.ChangeQuery("");
                        //返回false告诉Wox不要隐藏查询窗体，返回true则会自动隐藏Wox查询窗口
                        return true;
                    }
                });
            }
            return results;
        }


        private string[] FilterStringList(string[] strList, string[] filterList)
        {
            List<string> retList = new List<string>();
            foreach(string str in strList)
            {
                bool bAllMatch = true;
                foreach(string fil in filterList)
                {
                    if(str.IndexOf(fil) < 0)
                    {
                        bAllMatch = false;
                        break;
                    }
                }
                if(bAllMatch)
                {
                    retList.Add(str);
                }
            }
            return retList.ToArray();
        }

        private string[] GetXshList()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_PATH_XSHELL_SESSIONS_FOLDER);
            var fileInfoList = dirInfo.GetFiles("*.xsh");
            List<string> strList = new List<string>();
            foreach(var fileInfo in fileInfoList)
            {
                strList.Add(Path.GetFileNameWithoutExtension(fileInfo.Name));
            }
            return strList.ToArray();
        }

    }
}
