#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using MathNet.Numerics.Transformations;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "FFT", Category = "Value", Version = "", Help = "Generic FFT transformation on Spreads", Tags = "Analysis, DSP")]
	#endregion PluginInfo
	public class FFTValueNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public IDiffSpread<double> Buffer;
		
		[Input("Samplerate", MinValue = 1, DefaultValue = 60, IsSingle = true)]
		public IDiffSpread<int> Samplerate;
		
		[Output("Real Part")]
		public ISpread<double> FOutput;
		
		[Output("Imaginary Part")]
		public ISpread<double> FOutput2;
		
		[Output("Frequency Range")]
		public ISpread<double> FOut;
		
		[Output("Magnitude")]
		public ISpread<double> FMag;		
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(!Buffer.IsChanged) return;
			int BinSize = Buffer.SliceCount;
			
			double[] freqReal, freqImag;
			double[] dataReal = new double[Buffer.SliceCount];
			
			for (int i=0; i<BinSize; i++ ) {
				dataReal[i] = Buffer[i];
			}		
						
			RealFourierTransformation rft = new RealFourierTransformation(); // default convention
			rft.TransformForward(dataReal, out freqReal, out freqImag);
			
			FOutput.SliceCount = freqReal.Length;
			FOutput.AssignFrom(freqReal);
			
			FOutput2.SliceCount = freqImag.Length;
			FOutput2.AssignFrom(freqImag);
			
			FOut.SliceCount = FMag.SliceCount = BinSize/2;
			for (int i=1; i <= BinSize/2; i++) {
				FMag[i-1] = Math.Sqrt(freqReal[i] * freqReal[i] + freqImag[i] * freqImag[i]);
				FOut[i-1] = i * Samplerate[0] / (double) BinSize;
			}
			
		}
	}
}
