using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singijeon
{
    public partial class Form2 : Form
    {
        public AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        public Form2(AxKHOpenAPILib.AxKHOpenAPI openApiInstance)
        {
            axKHOpenAPI1 = openApiInstance;
            InitializeComponent();
        }
    }
}
