namespace Launchpad
{
    partial class LaunchpadView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BookmarkTreeview = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // BookmarkTreeview
            // 
            this.BookmarkTreeview.AllowDrop = true;
            this.BookmarkTreeview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BookmarkTreeview.Location = new System.Drawing.Point(4, 4);
            this.BookmarkTreeview.Name = "BookmarkTreeview";
            this.BookmarkTreeview.Size = new System.Drawing.Size(791, 547);
            this.BookmarkTreeview.TabIndex = 0;
            // 
            // LaunchpadView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BookmarkTreeview);
            this.Name = "LaunchpadView";
            this.Size = new System.Drawing.Size(798, 554);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView BookmarkTreeview;

    }
}
