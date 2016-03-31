using System;
using System.Diagnostics;		// Conditional
using System.Text;

namespace Sodes.Base
{
    public class Traceable
    {
        const int maxSize =
#if DEBUG
            500000;
#else
            50000;
#endif
        private StringBuilder traceInfo = new StringBuilder(maxSize / 5, maxSize);
        private string indentation;
        private bool needIndent = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string GetTrace()
        {
            string returnInfo = string.Empty;
            try
            {
                returnInfo = traceInfo.ToString();
            }
            catch
            {
            }

            try
            {
                traceInfo.Clear();
            }
            catch
            {
            }

            return returnInfo;
        }

        public bool Trace(string newTraceInfo)
        {
            InternalWrite(newTraceInfo, true);
            needIndent = true;
            return true;    // makes the trace usable in complex if expressions
        }

        public bool Trace(string newTraceInfo, params object[] args)
        {
            return Trace(string.Format(newTraceInfo, args));
        }

        [Conditional("TRACE")]
        protected void Assert(bool condition, string newTraceInfo)
        {
            if (!condition)
            {
                Trace(newTraceInfo);
            }
        }

        [Conditional("TRACE")]
        public void TraceWrite(string newTraceInfo)
        {
            InternalWrite(newTraceInfo, false);
        }

        [Conditional("TRACE")]
        public void TraceWrite(string newTraceInfo, params object[] args)
        {
            TraceWrite(string.Format(newTraceInfo, args));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), Conditional("TRACE")]
        private void InternalWrite(string newTraceInfo, bool addNewLine)
        {
            try
            {
                lock (traceInfo)
                {
                    newTraceInfo = newTraceInfo.Replace("\r\n", "\n");
                    string[] lines = newTraceInfo.Split('\n');
                    for (int line = 0, m = lines.Length; line < m; line++)
                    {
                        if (this.traceInfo.Length + lines[line].Length + 100 > maxSize)
                        {
                            this.traceInfo.Remove(20000, 9000);
                            traceInfo.Insert(20000, Environment.NewLine + "****** trace overflow" + Environment.NewLine);
                        }

                        if (needIndent || line > 0)
                        {
                            this.traceInfo.Append(this.indentation);
                            needIndent = false;
                        }

                        //if (lines[line].Length > this.traceInfo.Capacity) System.Diagnostics.Debugger.Break();
                        traceInfo.Append(lines[line]);

                        if (line < lines.Length - 1)
                        {
                            traceInfo.Append(Environment.NewLine);
                        }
                    }

                    if (addNewLine) traceInfo.Append(Environment.NewLine);
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                //System.Diagnostics.Debugger.Break();
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
#else
            catch (Exception)
            {
            }
#endif
            //System.Diagnostics.Debug.WriteLine(newTraceInfo);
        }

        [Conditional("TRACE")]
        protected void TraceIndent()
        {
            this.indentation += "  ";
        }

        [Conditional("TRACE")]
        protected void TraceUnindent()
        {
            this.indentation = this.indentation.Substring(2);
        }
    }
}
