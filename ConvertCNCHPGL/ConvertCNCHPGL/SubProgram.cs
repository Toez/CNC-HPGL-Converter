using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    public enum SubPrograms { BOX, SECTION }

    class SubProgram : FileCoordinateObject
    {
        private FileObject[] _Content;
        public FileObject[] Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            private set
            {
                _Name = value;
            }
        }

        private int _ContentIndex;
        public int ContentIndex
        {
            get
            {
                return _ContentIndex;
            }
            set
            {
                _ContentIndex = value;
            }
        }

        private SubPrograms _Program;
        public SubPrograms Program
        {
            get
            {
                return _Program;
            }
            set
            {
                _Program = value;
            }
        }

        private bool _InitializeNull;
        public bool InitializeNull
        {
            get
            {
                return _InitializeNull;
            }
            set
            {
                _InitializeNull = value;
            }
        }

        private bool _InnerSection;
        public bool InnerSection
        {
            get
            {
                return _InnerSection;
            }
            set
            {
                _InnerSection = value;
            }
        }

        private bool _MadeByProgram;
        public bool MadeByProgram
        {
            get
            {
                return _MadeByProgram;
            }
            set
            {
                _MadeByProgram = value;
            }
        }

        public SortedDictionary<int, SubProgram> InnerPrograms
        {
            get
            {
                SortedDictionary<int, SubProgram> result = new SortedDictionary<int, SubProgram>();

                for (int i = 0; i < Content.Length; i++)
                {
                    if (Content[i].GetType() == typeof(SubProgram))
                    {
                        result.Add(i, (SubProgram)Content[i]);
                    }
                }

                return result;
            }
        }

        public SubProgram()
            : this("", new Coordinate(), 0)
        {

        }

        public SubProgram(string name, Coordinate origen, int size)
            : this(name, origen, size, SubPrograms.SECTION)
        {

        }

        public SubProgram(string name, Coordinate origen, int size, SubPrograms program)
            : this(name, origen, size, program, 0)
        {
            
        }

        public SubProgram(string name, Coordinate origen, int size, SubPrograms program, int color)
            : this(name, origen, size, color, new SectionOptions(program, false, false, false))
        {
            
        }

        public SubProgram(string name, Coordinate origen, int color, SectionOptions options)
            : this(name, origen, 1000, color, options)
        {

        }

        public SubProgram(string name, Coordinate origen, int size, int color, SectionOptions options)
            : base(origen, color)
        {
            Content = new FileCoordinateObject[size];
            Name = name;
            Program = options.ProgramType;
            InitializeNull = options.InitializeNull;
            InnerSection = options.InnerSection;
            MadeByProgram = options.MadeByProgram;
        }

        public override void Write(Class_Output output)
        {
            try
            {
                switch (Program)
                {
                    case SubPrograms.BOX:
                        output.OutputBoxStart(Name.ToString());

                        if (InitializeNull)
                            output.OutputNullInstance();

                        if (InnerSection)
                            output.OutputSectionStart(Name, Name);
                        break;
                    case SubPrograms.SECTION:
                        output.OutputSectionStart(Name, Name);
                        break;
                    default:
                        output.OutputSectionStart(Name, Name);
                        break;
                }

                for (int i = 0; i < Content.Length; i++)
                {
                    if (Content[i] != null)
                        Content[i].Write(output);
                }

                switch (Program)
                {
                    case SubPrograms.BOX:
                        if (InnerSection)
                            output.OutputSectionEnd();

                        output.OutputBoxEnd();
                        break;
                    case SubPrograms.SECTION:
                        output.OutputSectionEnd();
                        break;
                    default:
                        output.OutputSectionEnd();
                        break;
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        public void AddContent(FileCoordinateObject item)
        {
            try
            {
                if (ContentIndex < Content.Length)
                    Content[ContentIndex] = item;
                else
                {
                    FileObject[] temp = new FileObject[ContentIndex + 1];
                    Array.Copy(Content, temp, Content.Length);
                    Content = temp;
                    Content[ContentIndex] = item;
                }

                ContentIndex++;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        public bool ProgramAlreadyExists(SubProgram prog)
        {
            bool result = false;

            try
            {
                if (Content.Contains(prog))
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public bool ContainsSubCall()
        {
            bool result = false;

            try
            {
                for (int i = 0; i < Content.Length; i++)
                {
                    if (Content[i].GetType() == typeof(SubCall))
                    {
                        result = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public bool ContainsCoordinates()
        {
            bool result = false;

            try
            {
                for (int i = 0; i < Content.Length; i++)
                {
                    if (!Statements.IsNull(Content[i]) && (Content[i].GetType() == typeof(Line) || Content[i].GetType() == typeof(Arc)))
                    {
                        result = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public bool ContainsContent()
        {
            bool result = false;

            try
            {
                for (int i = 0; i < Content.Length; i++)
                {
                    if (!Statements.IsNull(Content[i]))
                    {
                        if (Content[i].GetType().IsSubclassOf(typeof(FileCoordinateObject)))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }
    }

    public struct SectionOptions
    {
        public SubPrograms ProgramType;
        public bool InitializeNull;
        public bool InnerSection;
        public bool MadeByProgram;

        public SectionOptions(SubPrograms programType, bool initializeNull, bool innerSection, bool madeByProgram)
        {
            ProgramType = programType;
            InitializeNull = initializeNull;
            InnerSection = innerSection;
            MadeByProgram = madeByProgram;
        }
    }
}
