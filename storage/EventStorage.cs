

using SbekuMod.utils;

namespace SbekuMod.storage
{
    public class EventData
    {
        public bool HasSeenWelcomeScreen { get; set; } = false;
    }

    public class EventStorage : StorageHandler<EventData>
    {

        protected override string GetFilename() => "events.json";

    }
}
