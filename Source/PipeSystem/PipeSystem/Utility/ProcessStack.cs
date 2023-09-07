using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PipeSystem
{
    public class ProcessStack : IExposable
    {
        public const int MaxCount = 15;

        private List<Process> processes = new List<Process>();
        private Process currentProcess;

        /// <summary>
        /// Return first def that can be done
        /// </summary>
        public Process FirstCanDo
        {
            get
            {
                if (currentProcess == null)
                {
                    for (int i = 0; i < processes.Count; i++)
                    {
                        var process = processes[i];
                        if (process.ShouldDoNow())
                        {
                            currentProcess = process;
                            break;
                        }
                    }
                }
                return currentProcess;
            }
        }

        // Expose values:
        public List<Process> Processes => processes;

        public ProcessStack() { }

        /// <summary>
        /// Save processes
        /// </summary>
        public void ExposeData()
        {
            Scribe_References.Look(ref currentProcess, "currentProcess");
            Scribe_Collections.Look(ref processes, "processes", LookMode.Deep);
        }

        /// <summary>
        /// Allow iterating over stack
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Process> GetEnumerator() => processes.GetEnumerator();

        /// <summary>
        /// Set current process to null
        /// </summary>
        public void Notify_ProcessEnded()
        {
            currentProcess = null;
        }

        /// <summary>
        /// Set current process to null if it didn't start yet.
        /// Called when adding, removing or reordering processes in stack.
        /// Called when process target count change, when suspend status change.
        /// </summary>
        public void Notify_ProcessChange()
        {
            // Current process isn't started, we set it to null
            if (currentProcess != null && currentProcess.Def.ticks == currentProcess.TickLeft)
            {
                currentProcess = null;
            }
        }

        /// <summary>
        /// Add def to stack
        /// </summary>
        /// <param name="processDef"></param>
        /// <param name="parent"></param>
        public void AddProcess(ProcessDef processDef, ThingWithComps parent)
        {
            var process = new Process(processDef, parent);
            process.Setup();
            process.PostSpawnSetup();
            processes.Add(process);
            Notify_ProcessChange();
        }

        /// <summary>
        /// Delete process from stack
        /// </summary>
        /// <param name="process"></param>
        public void Delete(Process process)
        {
            if (processes.Contains(process))
                processes.Remove(process);
            // If current process is the one we delete and it didn't start, kill it
            if (process == currentProcess && process.Progress == 0f)
            {
                currentProcess = null;
            }
        }

        /// <summary>
        /// Reorder process
        /// </summary>
        /// <param name="process"></param>
        /// <param name="offset"></param>
        public void Reorder(Process process, int offset)
        {
            int num = processes.IndexOf(process);
            num += offset;
            if (num >= 0)
            {
                processes.Remove(process);
                processes.Insert(num, process);
            }
            Notify_ProcessChange();
        }

        /// <summary>
        /// Return index of process in stack
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public int IndexOf(Process process) => processes.IndexOf(process);
    }
}