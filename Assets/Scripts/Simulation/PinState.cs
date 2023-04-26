namespace DLS.Simulation
{
	public enum PinState { FLOATING =-1, LOW=0, HIGH=1 }

	public static class PinStateExtensions
	{
		public static uint ToUint(this PinState state)
		{
			return (uint)(state == PinState.HIGH ? 1 : 0);
		}
	}
}