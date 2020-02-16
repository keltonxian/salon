using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SQLiteExtension
{
	/// <summary>
	/// 所有的数据都以 key - value存
	/// </summary>
	public class SQLite {

		private static SQLite _instance;
		public static SQLite Instance{
			get{
				if(_instance == null){
					_instance = new SQLite();
				}
				return _instance;
			}
		}

		private SQLiteDB _db = new SQLiteDB();

		private SQLite(){
			_db = new SQLiteDB();
			Open();
		}

		public void Open(){
			_db.Open(Application.persistentDataPath + "/data.db");
			Debug.Log("SQLite>>> open db"+ Application.persistentDataPath + "/data.db" );
		}

		public void CreateTable(string tableName)
		{
			string sql = @"CREATE TABLE IF NOT EXISTS "+tableName+" (id INTEGER PRIMARY KEY, key TEXT, value TEXT);";
			SQLiteQuery qr = new SQLiteQuery(_db,sql);
			qr.Step();
			qr.Release();
		}

		public void DeleteTable(string tableName){
			CreateTable(tableName);
			string sql = @"DELETE FROM "+tableName;
			SQLiteQuery qr = new SQLiteQuery(_db,sql);
			qr.Step();
			qr.Release();
		}

		/// <summary>
		/// Add Or Update the specified tableName, key and value.
		/// </summary>
		/// <param name="tableName">Table name.</param>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetValue(string tableName , string key , string value)
		{
			CreateTable(tableName);
			string sql = "";
			SQLiteQuery qr = null;

			if(ContainKey(tableName,key)){
				sql = @"Update "+tableName+" SET value = ? where key = ?";
				qr = new SQLiteQuery(_db,sql);
				qr.Bind(value);
				qr.Bind(key);
			}else{
				sql = @"INSERT INTO "+tableName+" (key, value) VALUES (?,?);";
				qr = new SQLiteQuery(_db,sql);
				qr.Bind(key);
				qr.Bind(value);
			}
			qr.Step();
			qr.Release();
		}

		public void SetValue(string tableName , string key , int value)
		{
			SetValue(tableName,key,value.ToString());
		}
		public void SetValue(string tableName , string key , bool value)
		{
			SetValue(tableName,key,value?"1":"0");
		}
		public void SetValue(string tableName , string key , float value)
		{
			SetValue(tableName,key,value.ToString());
		}
		public void SetValue(string tableName , string key , double value)
		{
			SetValue(tableName,key,value.ToString());
		}
		public void SetValue(string tableName , string key , long value)
		{
			SetValue(tableName,key,value.ToString());
		}

		public bool ContainKey(string tableName,string key)
		{
			CreateTable(tableName);
			string sql = @"SELECT COUNT(id) as c FROM "+tableName+" WHERE key = ?";
			SQLiteQuery qr = new SQLiteQuery(_db,sql);
			qr.Bind(key);
			qr.Step();
			int c = qr.GetInteger("c");
			qr.Release();
			return c>0;
		}

		public void Delete(string tableName,string key){
			CreateTable(tableName);
			string sql = @"DELETE FROM "+tableName+" WHERE key=?";
			SQLiteQuery qr = new SQLiteQuery(_db,sql);
			qr.Bind(key);
			qr.Step();
			qr.Release();
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="tableName">Table name.</param>
		/// <param name="key">Key.</param>
		public string GetValue(string tableName,string key , string defaultValue)
		{
			CreateTable(tableName);

			if(!ContainKey(tableName,key)){
				return defaultValue;
			}
			string value = null;
			string sql = @"SELECT value FROM "+tableName+" WHERE key = ?";
			SQLiteQuery qr = new SQLiteQuery(_db,sql);
			qr.Bind(key);
			if(qr.Step()){
				try{
					value = qr.GetString("value");
				}catch{
					Debug.LogError("SQLITE>>" + tableName + "   Key:"+key + "   是Null数据");
				}
			}
			qr.Release();
			return value;
		}

		public int GetValue(string tableName,string key , int defaultValue)
		{
			return int.Parse( GetValue(tableName,key,defaultValue.ToString()));
		}
		public float GetValue(string tableName,string key , float defaultValue)
		{
			return  float.Parse( GetValue(tableName,key,defaultValue.ToString()));
		}
		public long GetValue(string tableName,string key , long defaultValue)
		{
			return  long.Parse( GetValue(tableName,key,defaultValue.ToString()));
		}
		public double GetValue(string tableName,string key , double defaultValue)
		{
			return  double.Parse( GetValue(tableName,key,defaultValue.ToString()) );
		}
		public bool GetValue(string tableName,string key , bool defaultValue)
		{
			string val = GetValue(tableName,key,defaultValue?"1":"0");
			if(val.Equals("1")) return true;
			return false;
		}

		public void CloseDB(){
			_db.Close();
		}
	}
}