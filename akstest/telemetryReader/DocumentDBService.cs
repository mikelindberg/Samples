
using System.Threading.Tasks;

namespace telemetryReader
{
    public class DocumentDBService
    {
        private static DocumentDBService instance;

        private DocumentDBService()
        {

        }

        public static DocumentDBService Instance
        {
            get
            {
                if (instance == null)
                    instance = new DocumentDBService();

                return instance;
            }
        }
    }
}