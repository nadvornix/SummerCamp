﻿using System.Threading.Tasks;
using System.ComponentModel;

using GoodAI.Core;
using GoodAI.Core.Memory;
using GoodAI.Core.Nodes;
using GoodAI.Core.Task;
using GoodAI.Core.Utils;
using ManagedCuda;
using YAXLib;

namespace CWRNN.Tasks
{
    /// <summary>
    /// Task that calculate outputs.
    /// </summary>
    [Description("Feedforward task"), MyTaskInfo(OneShot = false)]
    public class CWFeedForwardTask : MyTask<CWRNNLayer>
    {
        /// <summary>
        /// Output vector is calculated according to the memory and input
        /// </summary>

        private MyCudaKernel m_feedForwardHiddenKernel;
        private MyCudaKernel m_feedForwardOutputKernel;


        public override void Init(int nGPU)
        {

            m_feedForwardHiddenKernel = MyKernelFactory.Instance.Kernel(nGPU, @"\CWFeedForwardKernel", "CWFeedforwardHiddenKernel");
            m_feedForwardHiddenKernel.SetupExecution(Owner.HIDDEN_UNITS);
            m_feedForwardHiddenKernel.SetConstantVariable("D_INPUT_UNITS", Owner.INPUT_UNITS);
            m_feedForwardHiddenKernel.SetConstantVariable("D_HIDDEN_UNITS", Owner.HIDDEN_UNITS);
            m_feedForwardHiddenKernel.SetConstantVariable("D_OUTPUT_UNITS", Owner.OUTPUT_UNITS);
            m_feedForwardHiddenKernel.SetConstantVariable("D_ACTIVATION_FUNCTION", (int)Owner.ACTIVATION_FUNCTION);

            m_feedForwardOutputKernel = MyKernelFactory.Instance.Kernel(nGPU, @"\CWFeedForwardKernel", "CWFeedforwardOutputKernel");
            m_feedForwardOutputKernel.SetupExecution(Owner.OUTPUT_UNITS);
            m_feedForwardOutputKernel.SetConstantVariable("D_INPUT_UNITS", Owner.INPUT_UNITS);
            m_feedForwardOutputKernel.SetConstantVariable("D_HIDDEN_UNITS", Owner.HIDDEN_UNITS);
            m_feedForwardOutputKernel.SetConstantVariable("D_OUTPUT_UNITS", Owner.OUTPUT_UNITS);
            m_feedForwardOutputKernel.SetConstantVariable("D_ACTIVATION_FUNCTION", (int)Owner.ACTIVATION_FUNCTION);

        }

        public override void Execute()
        {
            Owner.HiddenActivations.CopyToMemoryBlock(Owner.PreviousHiddenActivations, 0, 0, Owner.HiddenActivations.Count);
            
            m_feedForwardHiddenKernel.Run(
                 Owner.Input,
                 Owner.HiddenActivations,
                 Owner.PreviousHiddenActivations,
                 Owner.HiddenActivationDerivatives,
                 Owner.InputWeights,
                 Owner.RecurrentWeights,
                 Owner.Periods,
                 Owner.MySimulationSteps
                );

            m_feedForwardOutputKernel.Run(
                Owner.HiddenActivations,
                Owner.OutputActivations,
                Owner.OutputActivationDerivatives,
                Owner.OutputWeights,
                 Owner.MySimulationSteps
                );


            Owner.OutputActivations.CopyToMemoryBlock(Owner.Output, 0, 0, Owner.OUTPUT_UNITS);
        }
    }
}