﻿using System;
using System.Diagnostics;
using System.IO;
using GoodAI.Core;
using GoodAI.Core.Utils;
using ManagedCuda;
using Xunit;
using Xunit.Abstractions;

namespace CoreTests
{
    public abstract class KernelFactoryTestBase
    {
        #region Nested helper classes

        protected class TempFileLoader
            : IDisposable
        {
            private readonly string m_tempFolderName;
            private readonly string m_targetPath;

            public TempFileLoader(string filePath)
            {
                m_tempFolderName = Path.GetRandomFileName();
                Directory.CreateDirectory(m_tempFolderName);
                m_targetPath = Path.Combine(m_tempFolderName, Path.GetFileName(filePath));
                File.Copy(filePath, m_targetPath);
                Directory.SetCurrentDirectory(m_tempFolderName);
            }

            public void Dispose()
            {
                Directory.SetCurrentDirectory("..");
                File.Delete(m_targetPath);
                Directory.Delete(m_tempFolderName);
            }
        }

        private class DebugLogWriter
            : MyLogWriter
        {
            private readonly ITestOutputHelper m_output;

            public DebugLogWriter(ITestOutputHelper output)
            {
                m_output = output;
            }

            public void WriteLine(MyLogLevel level, string message)
            {
                m_output.WriteLine(message + '\n');
            }

            public void Write(MyLogLevel level, string message)
            {
                m_output.WriteLine(message);
            }

            public void Write(MyLogLevel level, char message)
            {
                m_output.WriteLine(message.ToString());
            }

            public void FlushCache()
            { }
        }

        #endregion

        #region Fields and consts

        // Ptx names
        private readonly string m_ptxBase = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ptx");

        private const string BasicPtxName = "BasicTest";
        private const string DynParaPtxName = "DynParaTest";

        private const string EntryName = "Test";

        // Cuda names
        private const string CudaVar = "CUDA_PATH";

        protected static string CudaPath
        {
            get { return Environment.GetEnvironmentVariable(CudaVar); }
            set { Environment.SetEnvironmentVariable(CudaVar, value); }
        }

        protected string RtLibPath => Path.Combine($"{CudaPath}", "lib", "x64", "cudadevrt.lib");

        #endregion

        #region Genesis

        public KernelFactoryTestBase(ITestOutputHelper output)
        {
            MyLog.Writer = new DebugLogWriter(output);
        }

        #endregion

        #region Helpers

        protected MyCudaKernel GetKernel(bool dynamic, bool extendedLinkage)
        {
            string ptxName = dynamic ? DynParaPtxName : BasicPtxName;
            return MyKernelFactory.Instance.Kernel(0, m_ptxBase, ptxName, EntryName, true, extendedLinkage);
        }

        #endregion

        #region Simple loading

        [Fact]
        public void LoadBasicSimple()
        {
            var k = GetKernel(false, false);
            Assert.NotNull(k);
        }

        [Fact]
        public void LoadBasicExtended()
        {
            var k = GetKernel(false, true);
            Assert.NotNull(k);
        }

        [Fact]
        public virtual void LoadDynamicExtended()
        {
            var k = GetKernel(true, true);
            Assert.NotNull(k);
        }

        [Fact]
        public virtual void LoadDynamicBasic()
        {
            // The loading should fall back to extended linkage and should not fail
            var k = GetKernel(true, false);
            Assert.NotNull(k);
        }

        #endregion
    }

    public class KernelFactoryToolsLibTests
        : KernelFactoryTestBase
    {
        public KernelFactoryToolsLibTests(ITestOutputHelper output)
            : base(output)
        { }
    }

    public class KernelFactoryLocalLibTests
        : KernelFactoryTestBase, IDisposable
    {
        private TempFileLoader m_libLoader;

        public KernelFactoryLocalLibTests(ITestOutputHelper output)
            : base(output)
        {
            m_libLoader = new TempFileLoader(RtLibPath); // NOTE: sets working dir in a temp folder (might cause issues in the future)
        }

        public void Dispose()
        {
            m_libLoader.Dispose();
            m_libLoader = null;
        }
    }

    public class KernelFactoryNoLibTests
        : KernelFactoryTestBase, IDisposable
    {
        private readonly string m_oldCudaPath;

        public KernelFactoryNoLibTests(ITestOutputHelper output)
            : base(output)
        {
            m_oldCudaPath = CudaPath;
            CudaPath = "__INVALID_PATH"; // Making the cuda path invalid causes the lib not to be found
        }

        public void Dispose()
        {
            CudaPath = m_oldCudaPath;
        }


        #region Test overrides

        public override void LoadDynamicExtended()
        {
            Assert.Throws<CudaException>(() => GetKernel(true, true));
        }

        public override void LoadDynamicBasic()
        {
            Assert.Throws<CudaException>(() => GetKernel(true, false));
        }

        #endregion
    }
}
