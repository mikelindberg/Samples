using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace controller_interface_module
{

    public enum CommandType
    {
        //Test
        Test,
        //Move Arm
        mArm,
        //Move Hand
        mHand,
        //Rotate body
        rBody
    }

    public class Command
    {
        public Command(CommandType commandType, object payload)
        {
            type = commandType;
            data = payload;
        }

        // Returns the text of the enum instead of number
        [JsonConverter(typeof(StringEnumConverter))]
        public CommandType type { get; set; }

        public object data { get; set; }
    }
}