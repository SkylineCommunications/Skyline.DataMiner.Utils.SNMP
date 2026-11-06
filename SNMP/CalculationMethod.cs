namespace Skyline.DataMiner.Utils.SNMP
{
	/// <summary>The method to be used for delta tracking.</summary>
	public enum CalculationMethod
	{
		/// <summary>The delta is tracked on group level.</summary>
		Fast = 1,

		/// <summary>The delta is tracked on table row level.</summary>
		Accurate = 2,
	}
}
