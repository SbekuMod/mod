namespace SbekuMod.utils
{
    public abstract class StorageHandler<D> where D: new()
    {
        private D Data;

        public StorageHandler()
        {
            SetupStorage();
        }

        public D Get() => Data;

        public void Save()
        {
            SbekuMod.Instance.ModHelper.Storage.Save(Data, GetFilename());
        }

        private void SetupStorage()
        {
            Data = SbekuMod.Instance.ModHelper.Storage.Load<D>(GetFilename());

            if(Data == null) Data = new D();
        }

         protected abstract string GetFilename();

    }
}
