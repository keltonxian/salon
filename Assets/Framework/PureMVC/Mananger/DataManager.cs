using System;
using System.IO;
using System.Text;
using SQLiteExtension;
using UnityEngine;

namespace PureMVC.Manager
{
    public class DataManager : Manager
    {
        private static DataManager _instance;
        public static DataManager Instance
        {
            get
            {
                return _instance;
            }
        }

        void Awake()
        {
            _instance = this;
        }

        class StoragePath
        {
#if UNITY_EDITOR
            public static string streamingAssets = "file://" + Application.streamingAssetsPath + "/";
            public static string persistentData = "file://" + Application.persistentDataPath + "/";
            public static string temporaryCache = "file://" + Application.temporaryCachePath + "/";
#elif UNITY_ANDROID
		    public static string streamingAssets = Application.streamingAssetsPath + "/";
		    public static string persistentData = "file://" + Application.persistentDataPath + "/";
		    public static string temporaryCache = "file://" + Application.temporaryCachePath + "/";
#else
		    public static string streamingAssets = "file://" + Application.streamingAssetsPath + "/";
		    public static string persistentData = "file://" + Application.persistentDataPath + "/";
		    public static string temporaryCache = "file://" + Application.temporaryCachePath + "/";
#endif
        }

        public static string PathStreamingAssets
        {
            get
            {
                return StoragePath.streamingAssets;
            }
        }

        public static string PathPersistentData
        {
            get
            {
                return StoragePath.persistentData;
            }
        }

        public static string PathTemporaryCache
        {
            get
            {
                return StoragePath.temporaryCache;
            }
        }

        public static string FolderStreamingAssets
        {
            get
            {
                return "StreamingAssets";
            }
        }

        public static string FolderPersistentData
        {
            get
            {
                return "PersistentData";
            }
        }

        public static string FolderTemporaryCache
        {
            get
            {
                return "TemporaryCache";
            }
        }

        public static string FolderResources
        {
            get
            {
                return "Resources";
            }
        }

        public static int SByte2Int(int value)
        {
            return unchecked((byte)value);
            //return value < 0 ? 256 + value : value;
        }

        public static sbyte Int2SByte(int value)
        {
            value &= 0xFF;
            return unchecked((sbyte)value);
            //return (sbyte)(value > 127 ? -(256 - value) : value);
        }

        public static int Byte2Int(int value)
        {
            return value;
        }

        public static byte Int2Byte(int value)
        {
            value &= 0xFF;
            return (byte)value;
        }

        public static sbyte Byte2SByte(byte value)
        {
            return unchecked((sbyte)value);
            //return (sbyte)(value > 127 ? -(256 - value) : value);
        }

        public static byte SByte2Byte(sbyte value)
        {
            return unchecked((byte)value);
            //return (byte)(value >= 0 ? 127 + value : 256 + value);
        }

        public static int Short2Int(short value)
        {
            return value < 0 ? value + (short.MaxValue - short.MinValue + 1) : value;
        }

        public static short Byte2Short(byte value)
        {
            short temp = value;
            return (short)(temp < 0 ? 256 + temp : temp);
        }

        public static string UTF2String(byte[] bytes, int length, ref int startIndex)
        {
            int bufferLength = (length << 1);
            byte[] buffer = new byte[bufferLength];
            int bufferIndex = 0;
            int byteLength = length;
            int byteIndex = 0;
            int errorTimes = 0;

            for (; byteIndex < byteLength && bufferIndex < bufferLength;)
            {
                byte temp = bytes[startIndex + byteIndex];

                //  0x80 = 0xxx xxxx => 1 byte group
                if ((temp & 0x80) == 0)
                {
                    buffer[bufferIndex] = temp;
                    buffer[bufferIndex + 1] = 0;
                    //Debug.Log(string.Format("1 buffer[{0}]:[{1}], buffer[{2}]:[{3}]", bufferIndex, BitConverter.ToChar(buffer, bufferIndex), bufferIndex + 1, BitConverter.ToChar(buffer, bufferIndex + 1)));
                    byteIndex += 1;
                    bufferIndex += 2;
                    continue;
                }

                //  0xC0 = 110x xxxx => 2 bytes group
                if ((temp & 0xE0) == 0xC0)
                {
                    int tt = (((temp & 0x1F) << 6) | ((bytes[startIndex + byteIndex + 1]) & 0x3F));
                    buffer[bufferIndex] = (byte)(tt & 0xFF);
                    buffer[bufferIndex + 1] = (byte)((tt >> 8) & 0xFF);
                    //Debug.Log(string.Format("2 buffer[{0}]:[{1}], buffer[{2}]:[{3}]", bufferIndex, BitConverter.ToChar(buffer, bufferIndex), bufferIndex + 1, BitConverter.ToChar(buffer, bufferIndex + 1)));
                    byteIndex += 2;
                    bufferIndex += 2;
                    continue;
                }

                //  0xE0 = 1110 xxxx => 3 bytes group
                if ((temp & 0xF0) == 0xE0)
                {
                    int tt = (((temp & 0x0F) << 12) | (((bytes[startIndex + byteIndex + 1]) & 0x3F) << 6) | ((bytes[startIndex + byteIndex + 2]) & 0x3F));
                    buffer[bufferIndex] = (byte)(tt & 0xFF);
                    buffer[bufferIndex + 1] = (byte)((tt >> 8) & 0xFF);
                    //Debug.Log(string.Format("3 buffer[{0}]:[{1}], buffer[{2}]:[{3}]", bufferIndex, BitConverter.ToChar(buffer, bufferIndex), bufferIndex + 1, BitConverter.ToChar(buffer, bufferIndex + 1)));
                    byteIndex += 3;
                    bufferIndex += 2;
                    continue;
                }

                //  0xF0 = 1111 xxxx => Exception
                //  0xC0 = 10xx xxxx => Exception
                buffer[bufferIndex] = (byte)'?';
                buffer[bufferIndex + 1] = 0;
                //Debug.Log(string.Format("4 buffer[{0}]:[{1}], buffer[{2}]:[{3}]", bufferIndex, BitConverter.ToChar(buffer, bufferIndex), bufferIndex + 1, BitConverter.ToChar(buffer, bufferIndex + 1)));
                byteIndex += 1;
                bufferIndex += 2;
                errorTimes++;
            }

            string value = Encoding.Unicode.GetString(buffer, 0, bufferIndex);
            //Debug.Log("value:" + value);

            if (errorTimes > 0)
            {
                Debug.LogWarning(string.Format("UTF2String Exception {0} times", errorTimes));
                value = "[BUG]" + value;
            }

            startIndex += length;
            return value;
        }

        public static bool ReadBool(byte[] bytes, ref int startIndex)
        {
            bool value = (bytes[startIndex] == 1 ? true : false);
            startIndex++;
            return value;
        }

        public static byte ReadByte(byte[] bytes, ref int startIndex)
        {
            byte value = bytes[startIndex];
            startIndex++;
            return value;
        }

        public static short ReadShort(byte[] bytes, ref int startIndex)
        {
            byte[] temp = new byte[2];
            temp[0] = bytes[startIndex + 1];
            temp[1] = bytes[startIndex];
            short value = BitConverter.ToInt16(temp, 0);
            startIndex += 2;
            return value;
        }

        public static int ReadInt(byte[] bytes, ref int startIndex)
        {
            byte[] temp = new byte[4];
            temp[0] = bytes[startIndex + 3];
            temp[1] = bytes[startIndex + 2];
            temp[2] = bytes[startIndex + 1];
            temp[3] = bytes[startIndex];
            int value = BitConverter.ToInt32(temp, 0);
            startIndex += 4;
            return value;
        }

        public static long ReadLong(byte[] bytes, ref int startIndex)
        {
            byte[] temp = new byte[8];
            temp[0] = bytes[startIndex + 7];
            temp[1] = bytes[startIndex + 6];
            temp[2] = bytes[startIndex + 5];
            temp[3] = bytes[startIndex + 4];
            temp[4] = bytes[startIndex + 3];
            temp[5] = bytes[startIndex + 2];
            temp[6] = bytes[startIndex + 1];
            temp[7] = bytes[startIndex];
            long value = BitConverter.ToInt64(temp, 0);
            startIndex += 8;
            return value;
        }

        public static string ReadUTF(byte[] bytes, ref int startIndex)
        {
            short length = ReadShort(bytes, ref startIndex);
            if (length < 0)
            {
                return null;
            }
            if (length == 0)
            {
                return "";
            }
            if (length == 1)
            {
                return string.Format("{0}", (char)ReadByte(bytes, ref startIndex));
            }

            string value = UTF2String(bytes, length, ref startIndex);
            return value;
        }

        public static void SkipUTF(byte[] bytes, ref int startIndex)
        {
            short length = ReadShort(bytes, ref startIndex);
            startIndex += length;
        }

        public static void ReadSkip(byte[] bytes, int length, ref int startIndex)
        {
            startIndex += length;
        }

        public static string GetFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            int lastIndex = fileName.IndexOf('.');
            if (lastIndex < 0)
            {
                return fileName;
            }

            int beginIndex = fileName.IndexOf('/');
            if (beginIndex < 0)
            {
                beginIndex = 0;
            }
            return fileName.Substring(beginIndex, lastIndex - beginIndex);
        }

        public static Texture2D LoadImageWithPalette(Data imageData, Data paletteData)
        {
            if (null == imageData)
            {
                return null;
            }
            if (null == paletteData)
            {
                Texture2D texTemp = new Texture2D(2, 2);
                texTemp.LoadImage(imageData.Bytes);
                return texTemp;
            }
            Data newImageData = ApplyPLETData(imageData, paletteData);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(newImageData.Bytes);
            return tex;
        }

        public static Data ApplyPLETData(Data imageData, Data paletteData)
        {
            if (null == imageData || null == paletteData)
            {
                Debug.Log("ApplyPLETData step 1");
                return null;
            }
            int imageLen = imageData.Length;
            int paletLen = paletteData.Length;
            sbyte[] imageByteData = new sbyte[imageLen];
            byte[] tempImageBytes = imageData.Bytes;
            for (int i = 0; i < imageLen; i++)
            {
                imageByteData[i] = Byte2SByte(tempImageBytes[i]);
            }
            sbyte[] palletteByteData = new sbyte[paletLen];
            byte[] tempPalletteBytes = paletteData.Bytes;
            for (int i = 0; i < paletLen; i++)
            {
                palletteByteData[i] = Byte2SByte(tempPalletteBytes[i]);
            }

            sbyte[] newImageByteData = new sbyte[imageLen];

            Array.Copy(imageByteData, newImageByteData, imageLen);
            //for (int i = 0; i < imageLen; i++)
            //{
            //    newImageByteData[i] = imageByteData[i];
            //}

            Vector2 pletDataInfo = GetPLETDataOfPosAndLength(newImageByteData, imageLen);
            if (Math.Abs(pletDataInfo.x) < float.Epsilon && Math.Abs(pletDataInfo.y) < float.Epsilon)
            {
                Debug.Log("ApplyPLETData step 2");
                return null;
            }
            //Debug.Log("pletDataInfo.x:" + pletDataInfo.x);
            //Debug.Log("pletDataInfo.y:" + pletDataInfo.y);

            //int tx = (int)pletDataInfo.x;
            //Debug.Log("letDataInfo.x:" + pletDataInfo.x);
            //Debug.Log("letDataInfo.x int:" + tx);
            Array.Copy(palletteByteData, 0, newImageByteData, (int)pletDataInfo.x, paletLen);
            //for (int s = 0, t = (int)pletDataInfo.x; s < paletLen; s++, t++)
            //{
            //    newImageByteData[t] = palletteByteData[s];
            //}
            int tt = (int)pletDataInfo.x + paletLen;
            //Debug.Log(string.Format("newImageByteData[{0}][{1}]", tt - 5, newImageByteData[tt - 5]));
            //Debug.Log(string.Format("newImageByteData[{0}][{1}]", tt - 4, newImageByteData[tt - 4]));
            //Debug.Log(string.Format("newImageByteData[{0}][{1}]", tt - 3, newImageByteData[tt - 3]));
            //Debug.Log(string.Format("newImageByteData[{0}][{1}]", tt - 2, newImageByteData[tt - 2]));
            //Debug.Log(string.Format("newImageByteData[{0}][{1}]", tt - 1, newImageByteData[tt - 1]));

            int crc_data = GetCRC(newImageByteData, (int)pletDataInfo.x - 4, (int)pletDataInfo.y + 4);
            int crc_pos = (int)(pletDataInfo.x + pletDataInfo.y);
            //Debug.Log("crc_data:" + crc_data);
            //Debug.Log("crc_pos:" + crc_pos);

            newImageByteData[crc_pos] = Int2SByte(crc_data >> 24);
            newImageByteData[crc_pos + 1] = Int2SByte(crc_data >> 16);
            newImageByteData[crc_pos + 2] = Int2SByte(crc_data >> 8);
            newImageByteData[crc_pos + 3] = Int2SByte(crc_data);
            //Debug.Log("crc_pos 1:" + newImageByteData[crc_pos]);
            //Debug.Log("crc_pos 2:" + newImageByteData[crc_pos + 1]);
            //Debug.Log("crc_pos 3:" + newImageByteData[crc_pos + 2]);
            //Debug.Log("crc_pos 4:" + newImageByteData[crc_pos + 3]);

            //byte[] imageByteData = imageData.Bytes;
            //byte[] palletteByteData = palettedata.Bytes;

            //byte[] newImageByteData = new byte[imageLen];

            //Array.Copy(imageByteData, newImageByteData, imageLen);

            //Vector2 pletDataInfo = GetPLETDataOfPosAndLength(newImageByteData, imageLen);
            //if (Math.Abs(pletDataInfo.x) < float.Epsilon && Math.Abs(pletDataInfo.y) < float.Epsilon)
            //{
            //    Debug.Log("ApplyPLETData step 2");
            //    return null;
            //}

            //Array.Copy(palletteByteData, 0, newImageByteData, (int)pletDataInfo.x, paletLen);

            //int crc_data = GetCRC(newImageByteData, (int)pletDataInfo.x - 4, (int)pletDataInfo.y + 4);
            //int crc_pos = (int)(pletDataInfo.x + pletDataInfo.y);

            //newImageByteData[crc_pos] = (byte)(crc_data >> 24);
            //newImageByteData[crc_pos + 1] = (byte)(crc_data >> 16);
            //newImageByteData[crc_pos + 2] = (byte)(crc_data >> 8);
            //newImageByteData[crc_pos + 3] = (byte)crc_data;

            byte[] bytes = new byte[newImageByteData.Length];
            for (int i = 0; i < newImageByteData.Length; i++)
            {
                bytes[i] = SByte2Byte(newImageByteData[i]);
            }

            Data newData = new Data(bytes);
            return newData;
        }

        public static Vector2 GetPLETDataOfPosAndLength(sbyte[] imageByteData, int dataLen)
        {
            if (null == imageByteData)
            {
                return Vector2.zero;
            }
            Debug.Log("======kelton=====");
            for (int i = 0; i < dataLen; i++)
            {
                if (imageByteData[i] == 'P' && imageByteData[i + 1] == 'L' && imageByteData[i + 2] == 'T' && imageByteData[i + 3] == 'E')
                {
                    int len = Byte2Int(imageByteData[i - 4] << 24 | Byte2Int(imageByteData[i - 3] << 16 | Byte2Int(imageByteData[i - 2] << 8 | Byte2Int(imageByteData[i - 1]))));
                    Debug.Log("len: " + len);
                    return new Vector2(i + 4, len);
                }
            }
            Debug.Log("======kelton=====");
            //for (int i = 0; i < dataLen; i++)
            //{
            //    if (imageByteData[i] == 'P' && imageByteData[i + 1] == 'L' && imageByteData[i + 2] == 'T' && imageByteData[i + 3] == 'E')
            //    {
            //        int len = Byte2Int(imageByteData[i - 4] << 24 | Byte2Int(imageByteData[i - 3] << 16 | Byte2Int(imageByteData[i - 2] << 8 | Byte2Int(imageByteData[i - 1]))));
            //        return new Vector2(i + 4, len);
            //    }
            //}
            return Vector2.zero;
        }

        private static long[] crc_table;
        private static void init_crc_table()
        {
            crc_table = new long[256];
            for (int n = 0; n < 256; n++)
            {
                long c = n;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                    {
                        c = 0xEDB88320L ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }
                crc_table[n] = c;
            }
        }
        public static int GetCRC(sbyte[] buffData, int start, int length)
        {
            if (null == crc_table)
            {
                init_crc_table();
            }
            long c = 0xFFFFFFFFL;
            for (int i = start; i < start + length; i++)
            {
                c = (crc_table[(int)((c ^ buffData[i]) & 0xFF)] ^ (c >> 8));
            }
            c ^= 0xFFFFFFFFL;
            return (int)c;
        }

        public static Data GetFileData(string folder, string fileName, string type)
        {
            string fname = (string.IsNullOrEmpty(type) ? fileName : fileName + type);
            string path = string.Format("Assets/{0}/{1}{2}", FolderStreamingAssets, folder, fname);
            if (!File.Exists(path))
            {
                return null;
            }
            FileStream fs = new FileStream(path, FileMode.Open);
            fs.Seek(0, SeekOrigin.Begin);
            byte[] dd = new byte[fs.Length];
            fs.Read(dd, 0, (int)fs.Length);
            Data data = new Data(dd);
            fs.Close();
            fs.Dispose();
            return data;
        }

        private const string _tableName = "SALON_TABLE";

        public void SaveString (string key, string value)
        {
            SQLite.Instance.SetValue(_tableName, key, value);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return SQLite.Instance.GetValue(_tableName, key, defaultValue);
        }

        public void SaveInt(string key, int value)
        {
            SQLite.Instance.SetValue(_tableName, key, value);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return SQLite.Instance.GetValue(_tableName, key, defaultValue);
        }

        public void SaveFloat(string key, float value)
        {
            SQLite.Instance.SetValue(_tableName, key, value);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return SQLite.Instance.GetValue(_tableName, key, defaultValue);
        }

        public void SaveLong(string key, long value)
        {
            SQLite.Instance.SetValue(_tableName, key, value);
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            return SQLite.Instance.GetValue(_tableName, key, defaultValue);
        }

        public void SaveDouble(string key, double value)
        {
            SQLite.Instance.SetValue(_tableName, key, value);
        }

        public double GetDouble(string key, double defaultValue = 0)
        {
            return SQLite.Instance.GetValue(_tableName, key, defaultValue);
        }

        public void SaveBool(string key, bool value)
        {
            SQLite.Instance.SetValue(_tableName, key, value);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return SQLite.Instance.GetValue(_tableName, key, defaultValue);
        }
    }
}

