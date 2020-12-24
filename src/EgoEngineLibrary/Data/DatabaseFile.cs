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

    internal static class XmlNodeExtensions
    {
        public static XmlNode ChildNode(this XmlNode? parent, int index)
        {
            var child = parent?.ChildNodes[index];
            if (child is null)
                throw new InvalidOperationException($"Could not get child at index {index} from xml node {parent?.Name}.");
            return child;
        }

        public static XmlAttribute Attribute(this XmlNode? parent, string name)
        {
            var child = parent?.Attributes?[name];
            if (child is null)
                throw new InvalidOperationException($"Could not find attribute {name} from xml node {parent?.Name}.");
            return child;
        }
    }

    internal static class DataSetExtensions
    {
        public static DataTable Table(this DataSet set, string name)
        {
            var table = set?.Tables[name];
            if (table is null)
                throw new InvalidOperationException($"Could not find table {name} in data set.");
            return table;
        }
    }

    internal static class DataColumnExtensions
    {
        public static DataColumn Column(this DataTable? table, string name)
        {
            var col = table?.Columns[name];
            if (col is null)
                throw new InvalidOperationException($"Could not find column {name} in table {table?.TableName}.");
            return col;
        }

        public static DataRow Row(this DataTable? table, int index)
        {
            var row = table?.Rows[index];
            if (row is null)
                throw new InvalidOperationException($"Could not find row at index {index} in table {table?.TableName}.");
            return row;
        }
    }

    internal static class DataRowExtensions
    {
        public static T Items<T>(this DataRow? row, int index)
            where T : struct
        {
            var item = row?.ItemArray[index];
            if (item is not T val)
                throw new InvalidOperationException($"Could not find item at index {index} in data row.");
            return val;
        }

        public static T Itemc<T>(this DataRow? row, int index)
            where T : class
        {
            var item = row?.ItemArray[index];
            if (item is not T val)
                throw new InvalidOperationException($"Could not find item at index {index} in data row.");
            return val;
        }
    }

    [Serializable]
    public class DatabaseFile : DataSet
    {
        private List<string[]> _loadErrors;

        public DatabaseFile()
        {
            this._loadErrors = new List<string[]>();
        }

        public DatabaseFile(string xmlPath) : this()
        {
            string path = Path.GetFullPath(xmlPath).Replace(Path.GetFileName(xmlPath), string.Empty) + Path.GetFileNameWithoutExtension(xmlPath) + "_schema.xsd";
            if (File.Exists(path))
            {
                base.ReadXmlSchema(path);
            }
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
                                    row.Table.PrimaryKey = Array.Empty<DataColumn>();
                                    this._loadErrors[this._loadErrors.Count - 1][2] = "The rows were kept but it is recommended that you fix the problem.";
                                    handledErrors.Add(row.RowError);
                                }
                                catch (Exception exception2)
                                {
                                    (this._loadErrors[this._loadErrors.Count - 1])[1] += " " + exception2.Message;
                                    (this._loadErrors[this._loadErrors.Count - 1])[1] += " The row was removed. Below are its contents separated by \" | \". Make sure to fix the problem if you are going to add it again.";
                                    foreach (object? value in row.ItemArray)
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
                                    foreach (object? value in row.ItemArray)
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

        public DatabaseFile(string databasePath, string schemaPath) : this()
        {
            base.DataSetName = "database";
            using (DatabaseBinaryReader reader = new DatabaseBinaryReader(EndianBitConverter.Little, File.Open(databasePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Exception exception;
                Dictionary<DataColumn, string[]> dR = new Dictionary<DataColumn, string[]>();
                XmlDocument SXML = new XmlDocument();
                SXML.Load(File.Open(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                if (SXML.DocumentElement is null)
                    throw new InvalidDataException("The schema root xml element does not exist.");

                int itemNum = 0;
                int nodeNum = 0;
                base.DataSetName = reader.ReadUInt32().ToString();
                uint schemaVersion = 0;

                bool hasStringTable = false;
                if (SXML.DocumentElement.ChildNode(nodeNum).Name == "schemaVersion")
                {
                    uint schemaVersionXml = Convert.ToUInt32(SXML.DocumentElement.ChildNode(nodeNum).Attribute("version").Value);
                    schemaVersion = reader.ReadUInt32();

                    // Dirt 3 schemaVer, and schemaVerXml is different so don't do the error check
                    // Also D3 doesn't have a string table despite having a schemaVersion
                    if (schemaVersion != 3934935529) // Dirt has 3934935529 in db, and 3914959594 in schema
                    {
                        if (schemaVersion != schemaVersionXml)
                            throw new ArgumentException("The schema does not match with this database file.", nameof(schemaPath));

                        hasStringTable = true;
                    }

                    base.DataSetName = base.DataSetName + ";" + schemaVersion.ToString();
                    nodeNum++;
                }

                // Figure out the offset to the strings
                // this only applies to certain games
                // Does not apply to Dirt 3, despite having a schemaVersion
                // F12013 1000; GridAutosport 1407330540; Dirt3 3914959594, Dirt Rally 539233987;
                int offset = 12;
                if (hasStringTable)
                {
                    // Loop through the tables in the schema, and jump through the database file to find the string table offset
                    for (int i = 1; i < SXML.DocumentElement.ChildNodes.Count; ++i)
                    {
                        reader.Seek(offset, SeekOrigin.Begin);
                        int tableRowCount = reader.ReadInt32();
                        int fieldCount = SXML.DocumentElement.ChildNode(i).ChildNodes.OfType<XmlElement>().Count(x => x.Name == "field");
                        offset += ((tableRowCount * (fieldCount + 1)) * 4) + 8;
                    }
                    offset += 4;
                    reader.Seek(8, SeekOrigin.Begin);
                }
                
                // Loop through all the data table by table
                while (reader.BaseStream.Position < reader.BaseStream.Length &&
                    nodeNum < SXML.DocumentElement.ChildNodes.Count)
                {
                    reader.Seek(2, SeekOrigin.Current);
                    var tableID = reader.ReadUInt16();
                    if ((hasStringTable && tableID != 10795) || (!hasStringTable && tableID != 21570)) // 0x2A2B or 0x5442 (BT from LBT)
                    {
                        throw new ArgumentException("The schema does not match with this database file.", nameof(schemaPath));
                    }
                    itemNum = reader.ReadInt32();

                    // Setup DataTable with proper columns based on the schema
                    DataTable table = new DataTable(SXML.DocumentElement.ChildNode(nodeNum).Attribute("name").InnerText);
                    foreach (XmlElement element in SXML.DocumentElement.ChildNode(nodeNum))
                    {
                        if (element.Name != "field")
                        {
                            continue;
                        }

                        DataColumn column = new DataColumn();
                        table.Columns.Add(column);
                        column.ColumnName = element.Attribute("name").InnerText;
                        if (element.HasAttribute("key"))
                        {
                            if (element.Attribute("key").InnerText == "primary")
                            {
                                table.PrimaryKey = new DataColumn[] { column };
                            }
                            else
                            {
                                string[] strArray = element.Attribute("key").InnerText.Split(new char[] { '.' });
                                dR.Add(column, strArray);
                            }
                        }
                        switch (element.Attribute("type").InnerText)
                        {
                            case "float":
                                column.DataType = typeof(float);
                                column.DefaultValue = 0f;
                                break;
                            case "int":
                                column.DataType = typeof(int);
                                column.DefaultValue = 0;
                                break;
                            case "string":
                                column.DataType = typeof(string);
                                if (element.HasAttribute("size"))
                                {
                                    column.MaxLength = Convert.ToInt32(element.Attribute("size").InnerText);
                                }
                                column.DefaultValue = string.Empty;
                                break;
                            case "bool":
                                column.DataType = typeof(bool);
                                column.DefaultValue = false;
                                break;
                            default:
                                column.DataType = typeof(int);
                                column.DefaultValue = 0;
                                break;
                        }
                    }

                    // Read the data for this table
                    for (int num = 0; num < itemNum; num++)
                    {
                        var itmID = reader.ReadUInt16();
                        if (hasStringTable && itmID != 10797) // 0x2A2D
                        {
                            throw new ArgumentException("The schema does not match with this database file.", nameof(schemaPath));
                        }
                        reader.Seek(2, SeekOrigin.Current);

                        // Read the data for each field in the schema
                        DataRow row = table.NewRow();
                        List<object> list = new List<object>();
                        foreach (XmlElement element in SXML.DocumentElement.ChildNode(nodeNum))
                        {
                            if (element.Name != "field")
                            {
                                continue;
                            }

                            switch (element.Attribute("type").InnerText)
                            {
                                case "float":
                                    list.Add(reader.ReadSingle());
                                    break;
                                case "int":
                                    list.Add(reader.ReadInt32());
                                    break;
                                case "string":
                                    if (hasStringTable)
                                    {
                                        int returnPosition = (int)reader.BaseStream.Position + 4;
                                        reader.Seek(offset + reader.ReadInt32(), SeekOrigin.Begin);
                                        list.Add(reader.ReadTerminatedString(0));
                                        reader.Seek(returnPosition, SeekOrigin.Begin);
                                    }
                                    else
                                    {
                                        list.Add(reader.ReadDatabaseString(table.Column(element.Attribute("name").InnerText).MaxLength));
                                    }
                                    break;
                                case "bool":
                                    list.Add(reader.ReadBoolean());
                                    reader.ReadBytes(3);
                                    break;
                                default:
                                    list.Add(reader.ReadInt32());
                                    break;
                            }
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
                                    table.PrimaryKey = Array.Empty<DataColumn>();
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
                    if (pair.Key.Table is null)
                        throw new InvalidDataException($"Data column {pair.Key.ColumnName} does not have a data table.");
                    DataRelation relation = new DataRelation(pair.Key.Table.TableName + "." + pair.Key.ColumnName, this.Table(pair.Value[0]).Column(pair.Value[1]), pair.Key);
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
                DataTable table4 = this.Table(table3.TableName).Copy();
                if (this.Table(table4.TableName).PrimaryKey.Length == 0)
                {
                    if (this.Table(table4.TableName).Columns.Contains("ID"))
                    {
                        ordinal = this.Table(table4.TableName).Column("ID").Ordinal;
                        goto Label_01B2;
                    }
                    list.Add(table4.TableName);
                    continue;
                }
                ordinal = this.Table(table4.TableName).PrimaryKey[0].Ordinal;
            Label_01B2:
                num2 = 0;
                while (num2 < table4.Rows.Count)
                {
                    for (int j = 0; j < table3.Rows.Count; j++)
                    {
                        if (!((table3.Rows[j].RowState != DataRowState.Deleted) && table4.Row(num2).ItemArray[ordinal] == table3.Rows[j].ItemArray[ordinal]))
                        {
                            continue;
                        }
                        for (int k = 0; k < table3.Rows[j].ItemArray.Length; k++)
                        {
                            if (table4.Row(num2).ItemArray[k] != table3.Rows[j].ItemArray[k])
                            {
                                file.Table(table3.TableName).ImportRow(table3.Rows[j]);
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
                    file.Table(table3.TableName).ImportRow(row);
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

                uint schemaVersion = 0;
                foreach (string str in base.DataSetName.Split(new char[] { ';' }))
                {
                    writer.Write(Convert.ToUInt32(str));
                    schemaVersion = Convert.ToUInt32(str);
                }

                // Old style games, and Dirt 3's version don't have the table
                bool hasStringTable = schemaVersion != 1313096275 && schemaVersion != 3934935529;

                // Begin writing each table's data
                for (int i = 0; i < base.Tables.Count; i++)
                {
                    if (hasStringTable)
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

                    // Write each itm/row
                    foreach (DataRow row in base.Tables[i].Rows)
                    {
                        if (hasStringTable)
                        {
                            writer.Write((ushort)0x2a2d);
                            writer.Write((ushort)i);
                        }
                        else
                        {
                            writer.Write(Encoding.UTF8.GetBytes("ITM"));
                            writer.Write(Convert.ToByte(i));
                        }

                        // Write the itm/row's data
                        for (int j = 0; j < base.Tables[i].Columns.Count; j++)
                        {
                            if (base.Tables[i].Columns[j].DataType == typeof(float))
                            {
                                writer.Write(row.Items<float>(j));
                            }
                            else if (base.Tables[i].Columns[j].DataType == typeof(int))
                            {
                                writer.Write(row.Items<int>(j));
                            }
                            else if (base.Tables[i].Columns[j].DataType == typeof(string))
                            {
                                if (hasStringTable)
                                {
                                    if (dictionary.ContainsKey(row.Itemc<string>(j)))
                                    {
                                        writer.Write(dictionary[row.Itemc<string>(j)]);
                                    }
                                    else
                                    {
                                        writer.Write(list.Count);
                                        dictionary.Add(row.Itemc<string>(j), list.Count);
                                        list.AddRange(Encoding.UTF8.GetBytes(row.Itemc<string>(j)));
                                        list.Add(0);
                                    }
                                }
                                else
                                {
                                    writer.WriteDatabaseString(row.Itemc<string>(j), base.Tables[i].Columns[j].MaxLength);
                                }
                            }
                            else if (base.Tables[i].Columns[j].DataType == typeof(bool))
                            {
                                writer.Write(row.Items<bool>(j));
                                writer.Write(new byte[3]);
                            }
                        }
                    }
                }

                // Write the string table
                if (hasStringTable)
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
