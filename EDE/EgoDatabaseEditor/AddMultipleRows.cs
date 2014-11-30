using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EgoDatabaseEditor {
	public partial class AddMultipleRows : Form {
		public int Amount {
			get {
				try {
					return Convert.ToInt32(amountTextBox.Text);
				}
				catch {
					return -1;
				}
			}
		}

		public AddMultipleRows() {
			InitializeComponent();
		}
	}
}
