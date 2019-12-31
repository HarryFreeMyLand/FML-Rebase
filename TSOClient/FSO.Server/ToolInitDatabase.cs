﻿using FSO.Server.Common;
using FSO.Server.Database.DA;
using FSO.Server.Database.Management;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server
{
    /// <summary>
    /// This tool will install and update the SQL database.
    /// 
    /// It does this by reading the manifest.json file from the Scripts folder and compares it
    /// to whats inside the fso_db_changes table.
    /// 
    /// </summary>
    public class ToolInitDatabase : ITool
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();
        private IDAFactory DAFactory;

        public ToolInitDatabase(DatabaseInitOptions options, IDAFactory factory)
        {
            this.DAFactory = factory;
        }

        public int Run()
        {
            Console.WriteLine("Starting database init");

            using (var da = (SqlDA)DAFactory.Get())
            {
                var changeTool = new DbChangeTool(da.Context);
                var changes = changeTool.GetChanges();

                foreach(var change in changes)
                {
                    if(change.Status == DbChangeScriptStatus.MODIFIED && change.Idempotent == false)
                    {
                        Console.WriteLine(change.Status + " - " + change.ScriptFilename + " (Cant update, fix manually)");
                    }
                    else
                    {
                        Console.WriteLine(change.Status + " - " + change.ScriptFilename);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Apply changes (y|n)? Make sure you have backed up your database first");

                var input = Console.ReadLine().Trim();
                if (input.StartsWith("y") || input.StartsWith("r"))
                {
                    //Repair just updates fso_db_changes to latest
                    var repair = input.StartsWith("r");

                    Console.WriteLine("Applying changes");

                    foreach (var change in changes)
                    {
                        if(change.Status == DbChangeScriptStatus.FORCE_REINSTALL ||
                            change.Status == DbChangeScriptStatus.NOT_INSTALLED ||
                            (change.Status == DbChangeScriptStatus.MODIFIED && change.Idempotent))
                        {
                            try {
                                changeTool.ApplyChange(change, repair);
                            }catch(DbMigrateException e)
                            {
                                Console.Error.WriteLine("Error applying change: " + change.ScriptFilename);
                                Console.Error.WriteLine("\"" + e.Message + "\"");
                                Console.WriteLine("Would you like to continue? (y|n)?");
                                input = Console.ReadLine().Trim();
                                if (!input.StartsWith("y"))
                                {
                                    return -1;
                                }
                            }
                        }
                    }

                }
                else
                {
                    Console.WriteLine("No changes applied");
                }
            }
            return 0;
        }
    }
}
