namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public class CtfEntryInfo
    {
        public int id;
        public string name;
        public string type;
        private string minOperator;
        internal int minFlag;
        private string maxOperator;
        private int maxFlag;
        public int refID;
        public int linkID;
        public bool readOnly;
        public string description;
        public string[] restrictedValues;
        public object defaultValue
        {
            get
            {
                switch (type)
                {
                    case "int":
                        return 0;
                    case "float":
                        return 0.0f;
                    case "double":
                        return 0.0;
                    case "bool":
                        return false;
                    case "string":
                        return string.Empty;
                    case "float-list":
                        FloatList fList;
                        fList.count = 0;
                        fList.step = 0;
                        fList.items = new float[0];
                        return fList;
                    default:
                        return null;
                }
            }
        }
        public Type realType
        {
            get
            {
                switch (type)
                {
                    case "int":
                        return typeof(int);
                    case "float":
                        return typeof(float);
                    case "double":
                        return typeof(double);
                    case "bool":
                        return typeof(bool);
                    case "string":
                        return typeof(string);
                    case "float-list":
                        return typeof(FloatList);
                    default:
                        return null;
                }
            }
        }

        public CtfEntryInfo(int _id, XmlElement entry)
        {
            id = _id;
            name = entry.Attributes["name"].Value;
            type = entry.Attributes["type"].Value;
            minOperator = entry.GetAttribute("minOperator");
            if (entry.HasAttribute("minFlag"))
            {
                minFlag = Convert.ToInt32(entry.GetAttribute("minFlag"));
            }
            else
            {
                minFlag = -1;
            }
            maxOperator = entry.GetAttribute("maxOperator");
            if (entry.HasAttribute("maxFlag"))
            {
                maxFlag = Convert.ToInt32(entry.GetAttribute("maxFlag"));
            }
            else
            {
                maxFlag = -1;
            }
            if (entry.HasAttribute("refID"))
            {
                refID = Convert.ToInt32(entry.GetAttribute("refID"));
            }
            else
            {
                refID = -1;
            }
            if (entry.HasAttribute("linkID"))
            {
                linkID = Convert.ToInt32(entry.GetAttribute("linkID"));
            }
            else
            {
                linkID = -1;
            }
            readOnly = entry.HasAttribute("readOnly") ? true : false;
            List<string> rVals = new List<string>();
            foreach (XmlElement param in entry)
            {
                if (param.GetAttribute("name") == "description")
                {
                    description = param.InnerText;
                }
                else if (param.GetAttribute("name") == "restrictedValue")
                {
                    rVals.Add(param.InnerText);
                }
            }
            restrictedValues = rVals.ToArray();
        }

        public bool IsUsed(int flag)
        {
            if (minFlag != -1)
            {
                if (!PassOperation(flag, minOperator, minFlag))
                {
                    return false;
                }
            }
            if (maxFlag != -1)
            {
                if (!PassOperation(flag, maxOperator, maxFlag))
                {
                    return false;
                }
            }
            return true;
        }
        private bool PassOperation(int flag, string op, int targetFlag)
        {
            switch (op)
            {
                case "e":
                    if (flag == targetFlag)
                    {
                        return true;
                    }
                    break;
                case "lt":
                    if (flag < targetFlag)
                    {
                        return true;
                    }
                    break;
                case "lte":
                    if (flag <= targetFlag)
                    {
                        return true;
                    }
                    break;
                case "gt":
                    if (flag > targetFlag)
                    {
                        return true;
                    }
                    break;
                case "gte":
                    if (flag >= targetFlag)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
