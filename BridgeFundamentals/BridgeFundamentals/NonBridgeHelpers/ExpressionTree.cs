using System;
using System.Collections.Generic;
using System.Text;

namespace Sodes.Base
{
  public delegate string Factor2String(string factor);

	public class ExpressionTree<T>
  {
    private InternalExpression exp;

    public ExpressionTree(string rule)
    {
      // strip all spaces
      rule = rule.Replace(" ", "");
      int p = 0;
      exp = new InternalExpression(rule, ref p);
    }

    public string ToString(Factor2String translator)
    {
      return exp.ToString(translator);
    }

    protected delegate T Evaluate(string factor);

    private class InternalExpression
    {
      private List<OrExpression> or;

      public InternalExpression(string rule, ref int p)
      {
        or = new List<OrExpression>();
        bool more;
        do
        {
          or.Add(new OrExpression(rule, ref p));
          more = p < rule.Length && rule[p] == '+';
          if (more)
          {
            p++;
          }
        } while (more);
      }

      public string ToString(Factor2String translator)
      {
        string r = string.Empty;
        foreach (OrExpression x in or)
        {
          if (r.Length > 0)
          {
            r += "+";
          }

          r += x.ToString(translator);
        }
        return r;
      }

      private class OrExpression
      {
        private List<NotExpression> and;

        public OrExpression(string rule, ref int p)
        {
          and = new List<NotExpression>();
          bool more;
          do
          {
            and.Add(new NotExpression(rule, ref p));
            more = p < rule.Length && rule[p] == '*';
            if (more)
            {
              p++;
            }
          } while (more);
        }

        public string ToString(Factor2String translator)
        {
          string r = string.Empty;
          foreach (NotExpression x in and)
          {
            if (r.Length > 0)
            {
              r += "*";
            }

            r += x.ToString(translator);
          }
          return r;
        }

        private class NotExpression
        {
          private bool not;
          private SubExpression sub;

          public NotExpression(string rule, ref int p)
          {
            if (rule[p] == '!')
            {
              not = true;
              p++;
            }
            else
            {
              not = false;
            }
            sub = new SubExpression(rule, ref p);
          }

          public string ToString(Factor2String translator)
          {
            string r = string.Empty;
            if (not)
            {
              r += "!";
            }

            r += sub.ToString(translator);
            return r;
          }

          private class SubExpression
          {
            private bool isNode;
            private InternalExpression sub;
            private string factor;

            public SubExpression(string rule, ref int p)
            {
              if (rule[p] == '(')
              {
                isNode = false;
                p++;
                sub = new InternalExpression(rule, ref p);
                p++;    // to consume the closing )
              }
              else
              {
                isNode = true;
                factor = "";
                do
                {
                  factor += rule[p];
                  p++;
                } while (p < rule.Length && rule[p] != ')' && rule[p] != '+' && rule[p] != '*');

              }
            }

            public string ToString(Factor2String translator)
            {
              string r = string.Empty;
              if (isNode)
              {
                r += translator(factor);
              }
              else
              {
                r += "(" + sub.ToString(translator) + ")";
              }
              return r;
            }
          }
        }
      }
    }
  }
}