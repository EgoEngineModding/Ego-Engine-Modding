namespace EgoEngineLibrary.Data
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    [Serializable]
    public class DatabaseFile : DataSet
    {
        private List<string[]> _loadErrors;

        public DatabaseFile()
        {
            this._loadErrors = new List<string[]>();
        }

        public DatabaseFile(string xmlPath)
        {
            this._loadErrors = new List<string[]>();
            string path = Path.GetFullPath(xmlPath).Replace(Path.GetFileName(xmlPath), string.Empty) + Path.GetFileNameWithoutExtension(xmlPath) + "_schema.xsd";
            if (File.Exists(path))
            {
                base.ReadXmlSchema(path);
            }
            this._loadErrors = new List<string[]>();
            try
            {
                base.ReadXml(xmlPath, XmlReadMode.Auto);
            }
            catch (Exception exception)
            {
                if (!(exception is ConstraintException))
                {
                    throw new XmlException("There is an error in the xml!", exception);
                }
                HashSet<string> handledErrors = new HashSet<string>();
                for (int i = 0; i < base.Tables.Count; i++)
                {
                    List<DataRow> list = new List<DataRow>();
                    foreach (DataRow row in base.Tables[i].Rows)
                    {
                        if (row.HasErrors)
                        {
                            float num2;
                            int num3;
                            bool flag;
                            if (row.RowError.Contains("constrained to be unique") && !handledErrors.Contains(row.RowError))
                            {
                                try
                                {
                                    this._loadErrors.Add(new string[] { row.Table.TableName, row.RowError, string.Empty });
                                    row.Table.PrimaryKey = null;
                                    this._loadErrors[this._loadErrors.Count - 1][2] = "The rows were kept but it is recommended that you fix the problem.";
                                    handledErrors.Add(row.RowError);
                                }
                                catch (Exception exception2)
                                {
                                    (this._loadErrors[this._loadErrors.Count - 1])[1] += " " + exception2.Message;
                                    (this._loadErrors[this._loadErrors.Count - 1])[1] += " The row was removed. Below are its contents separated by \" | \". Make sure to fix the problem if you are going to add it again.";
                                    foreach (object value in row.ItemArray)
                                    {
                                        if (value is float)
                                        {
                                            num2 = (float)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += num2.ToString() + " | ";
                                        }
                                        else if (value is int)
                                        {
                                            num3 = (int)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += num3.ToString() + " | ";
                                        }
                                        else if (value is string)
                                        {
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += ((string)value) + " | ";
                                        }
                                        else if (value is bool)
                                        {
                                            flag = (bool)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += flag.ToString() + " | ";
                                        }
                                    }
                                    list.Add(row);
                                }
                            }
                            else
                            {
                                string str2;
                                if ((row.RowError.Contains("to exist in the parent table.") && !row.RowError.Contains("values (0)")) && !handledErrors.Contains(row.RowError))
                                {
                                    str2 = row.RowError.Remove(row.RowError.IndexOf(" requires")).Remove(0, 0x15);
                                    row.Table.Constraints.Remove(str2);
                                    this._loadErrors.Add(new string[] { row.Table.TableName, row.RowError + " It is recommended that you fix the problem. To help you find the row, it is displayed below:", string.Empty });
                                    foreach (object value in row.ItemArray)
                                    {
                                        if (value is float)
                                        {
                                            num2 = (float)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += num2.ToString() + " | ";
                                        }
                                        else if (value is int)
                                        {
                                            num3 = (int)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += num3.ToString() + " | ";
                                        }
                                        else if (value is string)
                                        {
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += ((string)value) + " | ";
                                        }
                                        else if (value is bool)
                                        {
                                            flag = (bool)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += flag.ToString() + " | ";
                                        }
                                    }
                                    handledErrors.Add(row.RowError);
                                }
                                else if (!(!row.RowError.Contains("values (0)") || handledErrors.Contains(row.RowError)))
                                {
                                    str2 = row.RowError.Remove(row.RowError.IndexOf(" requires")).Remove(0, 0x15);
                                    row.Table.Constraints.Remove(str2);
                                    handledErrors.Add(row.RowError);
                                }
                            }
                        }
                    }
                    foreach (DataRow row2 in list)
                    {
                        base.Tables[i].Rows.Remove(row2);
                    }
                }
            }
            try
            {
                base.EnforceConstraints = true;
            }
            catch
            {
            }
            base.AcceptChanges();
        }

        public DatabaseFile(string databasePath, string schemaPath)
        {
            this._loadErrors = new List<string[]>();
            base.DataSetName = "database";
            using (DatabaseBinaryReader reader = new DatabaseBinaryReader(EndianBitConverter.Little, File.Open(databasePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int num;
                Exception exception;
                Dictionary<DataColumn, string[]> dR = new Dictionary<DataColumn, string[]>();
                this._loadErrors = new List<string[]>();
                XmlDocument SXML = new XmlDocument();
                SXML.Load(File.Open(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                int itemNum = 0;
                int nodeNum = 0;
                base.DataSetName = reader.ReadUInt32().ToString();
                uint num4 = 0;
                if (SXML.DocumentElement.ChildNodes[nodeNum].Name == "schemaVersion")
                {
                    num4 = reader.ReadUInt32();
                    base.DataSetName = base.DataSetName + ";" + num4.ToString();
                    nodeNum++;
                }
                int offset = 12;
                if (num4 == 0x3e8)
                {
                    num = 1;
                    while (num < SXML.DocumentElement.ChildNodes.Count)
                    {
                        reader.Seek(offset, SeekOrigin.Begin);
                        int num6 = reader.ReadInt32();
                        offset += ((num6 * (SXML.DocumentElement.ChildNodes[num].ChildNodes.Count + 1)) * 4) + 8;
                        num++;
                    }
                    offset += 4;
                    reader.Seek(8, SeekOrigin.Begin);
                }
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    string innerText;
                    if (nodeNum >= SXML.DocumentElement.ChildNodes.Count)
                    {
                        break;
                    }
                    DataTable table = new DataTable(SXML.DocumentElement.ChildNodes[nodeNum].Attributes["name"].InnerText);
                    reader.Seek(4, SeekOrigin.Current);
                    itemNum = reader.ReadInt32();
                    foreach (XmlElement element in SXML.DocumentElement.ChildNodes[nodeNum])
                    {
                        DataColumn column = new DataColumn();
                        table.Columns.Add(column);
                        column.ColumnName = element.Attributes["name"].InnerText;
                        if (element.HasAttribute("key"))
                        {
                            if (element.Attributes["key"].InnerText == "primary")
                            {
                                table.PrimaryKey = new DataColumn[] { column };
                            }
                            else
                            {
                                string[] strArray = element.Attributes["key"].InnerText.Split(new char[] { '.' });
                                dR.Add(column, strArray);
                            }
                        }
                        innerText = element.Attributes["type"].InnerText;
                        if (innerText == null)
                        {
                            goto Label_03E8;
                        }
                        if (!(innerText == "float"))
                        {
                            if (innerText == "int")
                            {
                                goto Label_0346;
                            }
                            if (innerText == "string")
                            {
                                goto Label_036B;
                            }
                            if (innerText == "bool")
                            {
                                goto Label_03C6;
                            }
                            goto Label_03E8;
                        }
                        column.DataType = typeof(float);
                        column.DefaultValue = 0f;
                        continue;
                    Label_0346:
                        column.DataType = typeof(int);
                        column.DefaultValue = 0;
                        continue;
                    Label_036B:
                        column.DataType = typeof(string);
                        if (element.HasAttribute("size"))
                        {
                            column.MaxLength = Convert.ToInt32(element.Attributes["size"].InnerText);
                        }
                        column.DefaultValue = string.Empty;
                        continue;
                    Label_03C6:
                        column.DataType = typeof(bool);
                        column.DefaultValue = false;
                        continue;
                    Label_03E8:
                        column.DataType = typeof(int);
                        column.DefaultValue = 0;
                    }
                    for (num = 0; num < itemNum; num++)
                    {
                        DataRow row = table.NewRow();
                        List<object> list = new List<object>();
                        reader.Seek(4, SeekOrigin.Current);
                        foreach (XmlElement element in SXML.DocumentElement.ChildNodes[nodeNum])
                        {
                            innerText = element.Attributes["type"].InnerText;
                            if (innerText == null)
                            {
                                goto Label_05C0;
                            }
                            if (!(innerText == "float"))
                            {
                                if (innerText == "int")
                                {
                                    goto Label_0502;
                                }
                                if (innerText == "string")
                                {
                                    goto Label_051A;
                                }
                                if (innerText == "bool")
                                {
                                    goto Label_05A3;
                                }
                                goto Label_05C0;
                            }
                            list.Add(reader.ReadSingle());
                            continue;
                        Label_0502:
                            list.Add(reader.ReadInt32());
                            continue;
                        Label_051A:
                            if (num4 == 0x3e8)
                            {
                                int num7 = ((int)reader.BaseStream.Position) + 4;
                                reader.Seek(offset + reader.ReadInt32(), SeekOrigin.Begin);
                                list.Add(reader.ReadTerminatedString(0));
                                reader.Seek(num7, SeekOrigin.Begin);
                            }
                            else
                            {
                                list.Add(reader.ReadDatabaseString(table.Columns[element.Attributes["name"].InnerText].MaxLength));
                            }
                            continue;
                        Label_05A3:
                            list.Add(reader.ReadBoolean());
                            reader.ReadBytes(3);
                            continue;
                        Label_05C0:
                            list.Add(reader.ReadInt32());
                        }
                        row.ItemArray = list.ToArray();
                        try
                        {
                            table.Rows.Add(row);
                        }
                        catch (Exception exception2)
                        {
                            float num8;
                            int num9;
                            bool flag;
                            exception = exception2;
                            if (exception is ConstraintException)
                            {
                                try
                                {
                                    this._loadErrors.Add(new string[] { table.TableName, exception.Message, string.Empty });
                                    table.PrimaryKey = null;
                                    table.Rows.Add(row);
                                    this._loadErrors[this._loadErrors.Count - 1][2] = "The rows were kept but it is recommended that you fix the problem.";
                                }
                                catch (Exception exception3)
                                {
                                    (this._loadErrors[this._loadErrors.Count - 1])[1] += " " + exception3.Message;
                                    (this._loadErrors[this._loadErrors.Count - 1])[1] += " The row was removed. Below are its contents separated by \" | \". Make sure to fix the problem if you are going to add it again.";
                                    foreach (object value in list)
                                    {
                                        if (value is float)
                                        {
                                            num8 = (float)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += num8.ToString() + "|";
                                        }
                                        else if (value is int)
                                        {
                                            num9 = (int)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += num9.ToString() + "|";
                                        }
                                        else if (value is string)
                                        {
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += ((string)value) + "|";
                                        }
                                        else if (value is bool)
                                        {
                                            flag = (bool)value;
                                            (this._loadErrors[this._loadErrors.Count - 1])[2] += flag.ToString() + "|";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                this._loadErrors.Add(new string[] { table.TableName, exception.Message, string.Empty });
                                (this._loadErrors[this._loadErrors.Count - 1])[1] += " The row was removed. Below are its contents separated by \" | \". Make sure to fix the problem if you are going to add it again.";
                                foreach (object value in list)
                                {
                                    if (value is float)
                                    {
                                        num8 = (float)value;
                                        (this._loadErrors[this._loadErrors.Count - 1])[2] += num8.ToString() + "|";
                                    }
                                    else if (value is int)
                                    {
                                        num9 = (int)value;
                                        (this._loadErrors[this._loadErrors.Count - 1])[2] += num9.ToString() + "|";
                                    }
                                    else if (value is string)
                                    {
                                        (this._loadErrors[this._loadErrors.Count - 1])[2] += ((string)value) + "|";
                                    }
                                    else if (value is bool)
                                    {
                                        flag = (bool)value;
                                        (this._loadErrors[this._loadErrors.Count - 1])[2] += flag.ToString() + "|";
                                    }
                                }
                            }
                        }
                    }
                    base.Tables.Add(table);
                    nodeNum++;
                }
                foreach (KeyValuePair<DataColumn, string[]> pair in dR)
                {
                    DataRelation relation = new DataRelation(pair.Key.Table.TableName + "." + pair.Key.ColumnName, base.Tables[pair.Value[0]].Columns[pair.Value[1]], pair.Key);
                    pair.Key.Table.ExtendedProperties.Add(pair.Key.ColumnName, pair.Value[0]);
                    try
                    {
                        base.Relations.Add(relation);
                    }
                    catch (Exception exception4)
                    {
                        exception = exception4;
                        if ((exception is ConstraintException) && (exception.Message.Contains("to exist in the parent table.") && !exception.Message.Contains("values (0)")))
                        {
                            this._loadErrors.Add(new string[] { pair.Key.Table.TableName, exception.Message, string.Empty });
                            this._loadErrors[this._loadErrors.Count - 1][2] = "It is recommended that you fix the problem. To help you find the row, search for the value() from the above line in the " + pair.Key + " column.";
                        }
                    }
                }
            }
            base.Namespace = Path.GetFileName(schemaPath);
            base.AcceptChanges();
        }

        public DatabaseFile GetDifferences(DatabaseFile compare)
        {
            List<string> list = new List<string>();
            DatabaseFile file = new DatabaseFile
            {
                DataSetName = base.DataSetName,
                Namespace = base.Namespace
            };
            foreach (DataTable table in base.Tables)
            {
                file.Tables.Add(table.Clone());
            }
            file.EnforceConstraints = false;
            file.Relations.Clear();
            foreach (DataTable table2 in compare.Tables)
            {
                int ordinal;
                int num2;
                if (!base.Tables.Contains(table2.TableName))
                {
                    throw new ArgumentException("The files contain a different amount of tables!");
                }
                DataTable table3 = table2.Copy();
                DataTable table4 = base.Tables[table3.TableName].Copy();
                if (base.Tables[table4.TableName].PrimaryKey.Length == 0)
                {
                    if (base.Tables[table4.TableName].Columns.Contains("ID"))
                    {
                        ordinal = base.Tables[table4.TableName].Columns["ID"].Ordinal;
                        goto Label_01B2;
                    }
                    list.Add(table4.TableName);
                    continue;
                }
                ordinal = base.Tables[table4.TableName].PrimaryKey[0].Ordinal;
            Label_01B2:
                num2 = 0;
                while (num2 < table4.Rows.Count)
                {
                    for (int j = 0; j < table3.Rows.Count; j++)
                    {
                        if (!((table3.Rows[j].RowState != DataRowState.Deleted) && table4.Rows[num2].ItemArray[ordinal].Equals(table3.Rows[j].ItemArray[ordinal])))
                        {
                            continue;
                        }
                        for (int k = 0; k < table3.Rows[j].ItemArray.Length; k++)
                        {
                            if (!table4.Rows[num2].ItemArray[k].Equals(table3.Rows[j].ItemArray[k]))
                            {
                                file.Tables[table3.TableName].ImportRow(table3.Rows[j]);
                                break;
                            }
                        }
                        table3.Rows.RemoveAt(j);
                        break;
                    }
                    num2++;
                }
                foreach (DataRow row in table3.Rows)
                {
                    file.Tables[table3.TableName].ImportRow(row);
                }
            }
            string str = Environment.NewLine + Environment.NewLine;
            for (int i = 0; i < list.Count; i++)
            {
                str = str + list[i] + ", ";
            }
            str.Remove(str.Length - 2);
            //MessageBox.Show("NOTE: Deleted Rows and any rows from the following tables were not tested for:" + str, "Ryder Database Editor - Compare", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            file.AcceptChanges();
            return file;
        }

        public void Write(Stream savePath)
        {
            base.AcceptChanges();
            using (DatabaseBinaryWriter writer = new DatabaseBinaryWriter(EndianBitConverter.Little, savePath))
            {
                byte[] bytes = Encoding.UTF8.GetBytes("LBT");
                List<byte> list = new List<byte>();
                Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
                uint num = 0;
                foreach (string str in base.DataSetName.Split(new char[] { ';' }))
                {
                    writer.Write(Convert.ToUInt32(str));
                    num = Convert.ToUInt32(str);
                }
                for (int i = 0; i < base.Tables.Count; i++)
                {
                    if (num == 0x3e8)
                    {
                        writer.Write((ushort)i);
                        writer.Write((ushort)0x2a2b);
                    }
                    else
                    {
                        writer.Write(Convert.ToByte(i));
                        writer.Write(Encoding.UTF8.GetBytes("LBT"));
                    }
                    writer.Write(base.Tables[i].Rows.Count);
                    foreach (DataRow row in base.Tables[i].Rows)
                    {
                        if (num == 0x3e8)
                        {
                            writer.Write((ushort)0x2a2d);
                            writer.Write((ushort)i);
                        }
                        else
                        {
                            writer.Write(Encoding.UTF8.GetBytes("ITM"));
                            writer.Write(Convert.ToByte(i));
                        }
                        for (int j = 0; j < base.Tables[i].Columns.Count; j++)
                        {
                            if (base.Tables[i].Columns[j].DataType == typeof(float))
                            {
                                writer.Write((float)row.ItemArray[j]);
                            }
                            else if (base.Tables[i].Columns[j].DataType == typeof(int))
                            {
                                writer.Write((int)row.ItemArray[j]);
                            }
                            else if (base.Tables[i].Columns[j].DataType == typeof(string))
                            {
                                if (num == 0x3e8)
                                {
                                    if (dictionary.ContainsKey((string)row.ItemArray[j]))
                                    {
                                        writer.Write(dictionary[(string)row.ItemArray[j]]);
                                    }
                                    else
                                    {
                                        writer.Write(list.Count);
                                        dictionary.Add((string)row.ItemArray[j], list.Count);
                                        list.AddRange(Encoding.UTF8.GetBytes((string)row.ItemArray[j]));
                                        list.Add(0);
                                    }
                                }
                                else
                                {
                                    writer.WriteDatabaseString((string)row.ItemArray[j], base.Tables[i].Columns[j].MaxLength);
                                }
                            }
                            else if (base.Tables[i].Columns[j].DataType == typeof(bool))
                            {
                                writer.Write((bool)row.ItemArray[j]);
                                writer.Write(new byte[3]);
                            }
                        }
                    }
                }
                if (num == 0x3e8)
                {
                    writer.Write(Encoding.UTF8.GetBytes("PRTS"));
                    writer.Write(list.Count);
                    writer.Write(list.ToArray());
                }
            }
        }

        public void WriteXML(string savePath)
        {
            base.AcceptChanges();
            base.WriteXml(savePath, XmlWriteMode.WriteSchema);
        }

        public List<string[]> LoadErrors
        {
            get
            {
                return this._loadErrors;
            }
        }
    }
}
