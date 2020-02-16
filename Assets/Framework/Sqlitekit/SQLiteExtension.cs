using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Community.CsharpSqlite;


namespace SQLiteExtension
{

	public static class SQLiteExt 
	{
		public class Handle
        {
			public bool						Success;

			public string 					Path;
			public SQLiteQuery				Query;
			public SQLiteAsync              Async;
			public ThreadQueue.TaskControl  TaskControl;


			public override string ToString() {
				return string.Format("Success: {0}", Success);
			}
		}
        

		// Open Database
        public static IEnumerator SQLiteOpenDatabase<T>( this T self, string filename, Handle handle )
		{

			handle.Path = filename;
			handle.Async = SQLiteManager.Instance.GetSQLiteAsync(filename);

			handle.TaskControl = handle.Async.Open(filename,SQLiteExt_OpenCallback,handle);

			while(handle.TaskControl.Completed == false)
				yield return 0;
		}

        static void SQLiteExt_OpenCallback(bool succeed, object state)
        {
			var handle = (state as Handle);
			
			handle.Success = succeed;
        }


		// Close Database
		public static IEnumerator SQLiteCloseDatabase<T>( this T self, Handle handle )
		{
			
			handle.TaskControl = handle.Async.Close(SQLiteExt_CloseCallback,handle);
			
			while(handle.TaskControl.Completed == false)
				yield return 0;

		}


		static void SQLiteExt_CloseCallback(object state)
		{
			var handle = (state as Handle);
			handle.Success = true;
		}


		// Query 
		public static IEnumerator SQLiteQuery<T>( this T self, string query, Handle handle )
		{
			
			handle.TaskControl = handle.Async.Query(query,SQLiteExt_QueryCallback,handle);
			
			while(handle.TaskControl.Completed == false)
				yield return 0;
		}


		static void SQLiteExt_QueryCallback(SQLiteQuery qr, object state)
		{
			var handle = (state as Handle);
			handle.Query = qr;
			handle.Success = true;
		}



		// Step 
		public static IEnumerator SQLiteStep<T>( this T self, Handle handle )
		{
			
			handle.TaskControl = handle.Async.Step(handle.Query,SQLiteExt_StepCallback,handle);
			
			while(handle.TaskControl.Completed == false)
				yield return 0;
		}
		
		
		static void SQLiteExt_StepCallback(SQLiteQuery qr, bool rv, object state)
		{
			var handle = (state as Handle);
			handle.Success = rv;
		}


		// Release 
		public static IEnumerator SQLiteRelease<T>( this T self, Handle handle )
		{
			
			handle.TaskControl = handle.Async.Release(handle.Query,SQLiteExt_ReleaseCallback,handle);
			
			while(handle.TaskControl.Completed == false)
				yield return 0;
		}
		
		
		static void SQLiteExt_ReleaseCallback(object state)
		{
			var handle = (state as Handle);
			handle.Success = true;
		}

    }
}