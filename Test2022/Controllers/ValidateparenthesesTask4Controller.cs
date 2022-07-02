using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Test2022.Controllers
{
    public class ValidateparenthesesTask4Controller : Controller
    {
        // GET: ValidateparenthesesTask4
        public ActionResult ValidateparenthesesTask4View()
        {
            return View();
        }
        public ActionResult ValidateInput(string LISPCode)
        {
            int Num = CheckInputFile(LISPCode);

            if (Num == 0) return Json("Failed", JsonRequestBehavior.AllowGet);
            else return Json("Success", JsonRequestBehavior.AllowGet);

        }

        public int CheckInputFile(string LISPCode)
        {

            // Return 1 if string size is 0 
            if (LISPCode.Length == 0) return 1;

            Stack<char> brackets = new Stack<char>();

            foreach (char c in LISPCode)
            {
                if (c == '[' || c == '{' || c == '(')
                {
                    brackets.Push(c);
                }
                else
                {                    
                        if (c == ')')
                        {
                            if (brackets.Count > 0) // check if the stack count is greater then 0
                            {
                                if (brackets.Peek() == '(') brackets.Pop();
                                else return 0;
                            }
                             else return 0;
                        }

                        if (c == '}')
                        {
                            if (brackets.Count > 0) // check if the stack count is greater then 0
                            {
                                if (brackets.Peek() == '{') brackets.Pop();
                                else return 0;
                            }

                            else return 0;
                        }

                        if (c == ']')
                        {
                                if (brackets.Count > 0) // check if the stack count is greater then 0
                                {
                                    if (brackets.Peek() == '[') brackets.Pop();
                                else return 0;
                            }
                            else return 0;
                        }
                   

                }
            }

            // if there are no parantheses in the stack then it is valid string
            if (brackets.Count == 0) return 1;

            return 0;
        }
    }
}