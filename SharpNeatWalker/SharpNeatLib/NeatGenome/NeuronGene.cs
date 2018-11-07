using System;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.NeatGenome
{
	public class NeuronGene
	{
		// Although this id is allocated from the global innovation ID pool, neurons do not participate 
		// in compatibility measurements and so it is not used as an innovation ID. It is used as a unique
		// ID to distinguish between neurons.
		uint innovationId;
		NeuronType neuronType;
        IActivationFunction activationFunction;
        double neuronBias;
        double timeConstant; // GWM - For CTRNN architecture
        int timeCounter; // GWM - for SUPG architecture
        Boolean firstStepComplete;
        double xValue;
        double yValue;

		#region Constructor

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="copyFrom"></param>
		public NeuronGene(NeuronGene copyFrom)
		{
			this.innovationId = copyFrom.innovationId;
			this.neuronType = copyFrom.neuronType;
            this.activationFunction = copyFrom.activationFunction;
            this.neuronBias = copyFrom.neuronBias;
            this.timeConstant = copyFrom.timeConstant;
            this.timeCounter = copyFrom.timeCounter;
            this.xValue = copyFrom.xValue;
            this.yValue = copyFrom.yValue;
		}

		public NeuronGene(uint innovationId, NeuronType neuronType, IActivationFunction activationFunction)
		{
			this.innovationId = innovationId;
			this.neuronType = neuronType;
            this.activationFunction = activationFunction;
            this.neuronBias = 0;
            this.timeConstant = 1;
		}

		#endregion

		#region Properties

		public uint InnovationId
		{
			get
			{
				return innovationId;
			}
			set
			{
				innovationId = value;
			}
		}

		public NeuronType NeuronType
		{
			get
			{
				return neuronType;
			}
            set
            {
                neuronType = value;
            }
		}

        public IActivationFunction ActivationFunction
        {
            get
            {
                return activationFunction;
            }
            set
            {
                activationFunction = value;
            }
        }

        public double NeuronBias
        {
            get
            {
                return neuronBias;
            }
            set
            {
                neuronBias = value;
            }
        }

        public double TimeConstant
        {
            get
            {
                return timeConstant;
            }
            set
            {
                timeConstant = value;
            }
        }

        public int TimeCounter
        {
            get
            {
                return timeCounter;
            }
            set
            {
                timeCounter = value;
            }
        }

        public double XValue
        {
            get
            {
                return xValue;
            }
            set
            {
                xValue = value;
            }
        }

        public double YValue
        {
            get
            {
                return yValue;
            }
            set
            {
                yValue = value;
            }
        }

        public Boolean FirstStepComplete
        {
            get
            {
                return firstStepComplete;
            }
            set
            {
                firstStepComplete = value;
            }
        }

		#endregion
	}
}
