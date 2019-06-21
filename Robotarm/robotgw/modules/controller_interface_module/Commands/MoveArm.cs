namespace controller_interface_module
{
    public class MoveArm
    {
        //Step Pin
        public int sP { get; set; } = 0;

        //Direction Pin
        public int dP { get; set; } = 0;

        //Direction Value
        public byte dVal { get; set; }

        //Number of steps
        public int steps { get; set; } = 0;
    }
}