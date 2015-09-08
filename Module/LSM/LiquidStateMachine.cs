﻿using GoodAI.Core.Memory;
using GoodAI.Core.Nodes;
using GoodAI.Core.Task;
using GoodAI.Core.Utils;
using LSMModule.LSM.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace LSMModule {
    /// <author>Adr33</author>
    /// <meta>ok</meta>
    /// <status>Work in progress</status>
    /// <summary>Liquid State Machine node</summary>
    /// <description>
    /// Liquid State Machine node - the key node of this module<br></br>
    /// - tranfers binary input into an output, which also takes into account last few elements of the sequence
    /// - edges of the network are generated randomly with constrains put by different topologies
    /// - neurons are of type integrate-and-fire(IF)
    /// - recommended connectivity of LSM is said to be between 5-10% based on used topology
    /// - LSM can be either spiking or non-spiking
    /// - inner state and output of neurons are computed used equations discribed in LSMOutputTask
    /// - this implementation also allows you to make LSM spike internally more than once in one step
    /// - as output of this LSM we take current state of all the neurons in current step
    /// </description>
    class LiquidStateMachine : MyWorkingNode {


        public const float SPIKE_SIZE = 1;

        // Number of internal spiking steps in one external step
        public const int INNER_CYCLE = 1;

        [YAXSerializableField(DefaultValue = 0.1f)]
        [MyBrowsable, Category("\tLayer")]
        public virtual float Connectivity { get; set; }

        [YAXSerializableField(DefaultValue = 144)]
        [MyBrowsable, Category("\tLayer")]
        public virtual int Inputs { get; set; }

        [YAXSerializableField(DefaultValue = 0.5f)]
        [MyBrowsable, Category("\tLayer")]
        public virtual float Threshold { get; set; }

        [YAXSerializableField(DefaultValue = 1.0f)]
        [MyBrowsable, Category("\tLayer")]
        public virtual float A { get; set; }

        [YAXSerializableField(DefaultValue = 1.0f)]
        [MyBrowsable, Category("\tLayer")]
        public virtual float B { get; set; }

        [YAXSerializableField(DefaultValue = true)]
        [MyBrowsable, Category("\tLayer")]
        public virtual bool Spikes { get; set; }

        [YAXSerializableField(DefaultValue = 20)]
        [MyBrowsable, Category("Misc")]
        public int OutputColumnHint { get; set; }

        #region Memory blocks
        [MyInputBlock(0)]
        public MyMemoryBlock<float> Input {
            get { return GetInput(0); }
        }

        [MyOutputBlock(0)]
        public MyMemoryBlock<float> Output {
            get { return GetOutput(0); }
            set { SetOutput(0, value); }
        }

        public MyMemoryBlock<float> Weights { get; set; } //done
        public MyMemoryBlock<float> EdgeInputs { get; set; }
        public MyMemoryBlock<float> ImageInput { get; set; }
        public MyMemoryBlock<int> ImageOutput { get; set; }
        public MyMemoryBlock<float> InnerStates { get; set; }
        public MyMemoryBlock<float> NeuronOutputs { get; set; }
        public MyMemoryBlock<float> ImageSpikeProbabilities { get; set; }

        #endregion

        [MyTaskGroup("Init")]
        public LSMRandomInitTask RandomInitTask { get; private set; }
        [MyTaskGroup("Init")]
        public LSMMaassInitTask MaassInitTask { get; private set; }
        public LSMComputeTask ComputeTask { get; private set; }
        public LSMOutputTask OutputTask { get; private set; }

        public int Neurons;

        public override void UpdateMemoryBlocks() {

            // Calculates number of neurons based on used topology
            // Not sure whether this approach is correct, but it works

            if (RandomInitTask != null && RandomInitTask.Enabled) {
                Neurons = RandomInitTask.getNeurons();
            } else if (MaassInitTask != null && MaassInitTask.Enabled) {
                Neurons = MaassInitTask.getNeurons();
            } else {
                Neurons = 0;
            }

            Output.Count = Neurons;
            Output.ColumnHint = OutputColumnHint;

            Weights.Count = Neurons*Neurons;
            Weights.ColumnHint = Neurons;

            EdgeInputs.Count = Neurons*Neurons;
            Weights.ColumnHint = Neurons;

            ImageInput.Count = Neurons;
            ImageOutput.Count = Inputs;
            ImageInput.ColumnHint = OutputColumnHint;
            ImageOutput.ColumnHint = 12;

            InnerStates.Count = Neurons;
            NeuronOutputs.Count = Neurons;
            InnerStates.ColumnHint = OutputColumnHint;
            NeuronOutputs.ColumnHint = OutputColumnHint;

            ImageSpikeProbabilities.Count = Inputs;
            ImageSpikeProbabilities.ColumnHint = 24;

        }

    }
}
