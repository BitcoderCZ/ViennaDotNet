using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.ObjectStore.Server
{
    public class DataStore
    {
        private readonly DirectoryInfo rootDirectory;

        public DataStore(DirectoryInfo _rootDirectory)
        {
            rootDirectory = _rootDirectory;

            if (!rootDirectory.CanRead())
                throw new DataStoreException($"Data root directory {rootDirectory.FullName} is not a directory or cannot be read");

            if (!rootDirectory.Exists)
                rootDirectory.Create();
        }

        public string store(byte[] data)
        {
            string id = U.RandomUuid().ToString();

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(rootDirectory.FullName, id.Substring(0, 2)));
            if (!dir.Exists)
                dir.Create();

            FileInfo file = new FileInfo(Path.Combine(dir.FullName, id));

            try
            {
                using (FileStream fileOutputStream = file.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    fileOutputStream.Write(data);
            }
            catch (IOException ex)
            {
                file.Delete();
                throw new DataStoreException(ex);
            }

            return id;
        }

        public byte[]? load(string id)
        {
            FileInfo file = new FileInfo(Path.Combine(rootDirectory.FullName, id.Substring(0, 2), id));
            if (!file.Exists)
            {
                return null;
            }

            MemoryStream byteArrayOutputStream;
            try
            {
                byteArrayOutputStream = new MemoryStream((int)file.Length);
            }
            catch (IOException exception)
            {
                throw new DataStoreException(exception);
            }

            try
            {
                using (FileStream fileInputStream = file.OpenRead())
                    fileInputStream.CopyTo(byteArrayOutputStream);
            }

            catch (IOException exception)
            {
                throw new DataStoreException(exception);
            }

            byte[] data = byteArrayOutputStream.ToArray();

            return data;
        }

        public void delete(string id)
        {
            FileInfo file = new FileInfo(Path.Combine(rootDirectory.FullName, id.Substring(0, 2), id));
            file.Delete();
        }

        public class DataStoreException : Exception
        {
            public DataStoreException(string? message)
                : base(message)
            {
            }

            public DataStoreException(Exception? cause)
                : base(null, cause)
            {
            }
        }
    }
}
