using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D.Net.EmailClient;
using D.Net.EmailInterfaces;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Google.GData.Extensions;
using System.Text.RegularExpressions;
using Project_Status_Update_Survey_Helper.Properties;

namespace Project_Status_Update_Survey_Helper
{
    public partial class SurveyHelper : Form
    {
        private SpreadsheetEntry spreadsheet;
        
        //start the service
        private SpreadsheetsService service;

        private List<ProjectStatusUpdateWorksheet> worksheets;

        private List<ProjectStatusUpdateRow> newRows;
        private Dictionary<String, bool> newRowWorksheetsNeeded;

        public SurveyHelper()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.service = new SpreadsheetsService("Automation4Gabrielle");
            this.worksheets = new List<ProjectStatusUpdateWorksheet>();
            this.newRows = new List<ProjectStatusUpdateRow>();
            this.newRowWorksheetsNeeded = new Dictionary<string,bool>();
            this.txtUserName.Text = Settings.Default.signInEmail;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
            //reset the worksheets, but keep the "newRows" available, because we wont be able to reread the emails unless they're marked as unread manually.
            //if we didn't reset the worksheets, we'd probably have duplicates.
            this.worksheets = new List<ProjectStatusUpdateWorksheet>();

            txtLog.AppendText("Checking for Project Status Update emails..." + Environment.NewLine);
            IEmailClient ImapClient = EmailClientFactory.GetClient(EmailClientEnum.IMAP);
            string user = txtUserName.Text;
            string pass = txtPassword.Text;
            try{
            ImapClient.Connect("imap.gmail.com", user, pass, 993, true);            
            }
            catch(Exception exception)
            {
                txtLog.AppendText("There was an error specifically with connecting to your gmail account. (your pw is probably wrong)");
                txtPassword.Focus();
                txtPassword.SelectAll();
                return;
            }

            ImapClient.SetCurrentFolder("INBOX");
            // I assume that 5 is the last "ID" readed by my client. 

            //I have modified the D.Net library a little in order to allow me to use the settings properties of this windows form app
            ImapClient.LoadMessages("UNSEEN SUBJECT \"" + Settings.Default.emailSubject + "\" FROM \"" + Settings.Default.fromEmail + "\"");

            txtLog.AppendText(ImapClient.Messages.Count.ToString() + " project status update emails need to be parsed." + Environment.NewLine);

            List<IEmail> unreadMessages = ImapClient.Messages;

            // To read all loaded messages:
            int max = unreadMessages.Count;
            for (int i = 0; i < max; i++)
            {
                IEmail email = (IEmail)unreadMessages[i];

                //make sure we're dealing with the correct emails only!
                if (email.Subject.Contains(Settings.Default.emailSubject) && email.From.Contains(Settings.Default.fromEmail))
                {
                    email.LoadInfos();
                    ProjectStatusUpdateRow newRow = new ProjectStatusUpdateRow((String)email.TextBody, email.Date);
                    if (newRow.IsValid)
                    {
                        if (!newRowWorksheetsNeeded.ContainsKey(newRow.workSheetTitle))
                        {
                            newRowWorksheetsNeeded.Add(newRow.workSheetTitle, false);
                        }
                        newRows.Add(newRow);
                    }
                }
                else
                {
                    txtLog.AppendText("No unread project status update emails were found!" + Environment.NewLine);
                    break;
                }
            }

            startSpreadSheet(user, pass);
        }

        private void startSpreadSheet(String user, String pass)
        {
            //use clientlogin for authorization
            service.setUserCredentials(user, pass);

            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);

            txtLog.AppendText("Loading the spreadsheet..." + Environment.NewLine);

            // Iterate through all of the spreadsheets returned
            foreach (SpreadsheetEntry entry in feed.Entries)
            {
                // Print the title of this spreadsheet to the screen
                if (entry.Title.Text == Settings.Default.spreadsheetName)
                {
                    this.spreadsheet = entry;
                    break;
                }
            }
            if (this.spreadsheet == null)
            {
                txtLog.AppendText("Could not find a spreadsheet named " + Settings.Default.spreadsheetName +
                    ". Please update the spreadsheet name in the settings, or create the spreadsheet");
                return;
            }

            txtLog.AppendText("Spreadsheet loaded." + Environment.NewLine);

            String monthYearWsTitle = DateTime.Now.ToString("MMMM yyyy");
            bool addedEmailData = (newRows.Count > 0) ? false : true;

            WorksheetFeed wsFeed = this.spreadsheet.Worksheets;

            //loop over each worksheet in the spreadsheet we're dealing with and check for unsent surveys
            foreach (WorksheetEntry entry in wsFeed.Entries)
            {
                //WorksheetEntry item = (WorksheetEntry)wsFeed.Entries.Where(e => e.Title.Text == "something").Single();
                //Variable to keep track of whether we are in the right worksheet or not.
                
                //add data retreived from emails, if any, to the correct worksheet.
                if (newRowWorksheetsNeeded.ContainsKey(entry.Title.Text) && !newRowWorksheetsNeeded[entry.Title.Text])
                {
                    //append newRows to this worksheet.

                    // Define the URL to request the list feed of the worksheet.
                    AtomLink listFeedLink = entry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                    // Fetch the list feed of the worksheet.
                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = service.Query(listQuery);

                    foreach(ProjectStatusUpdateRow row in newRows)
                    {
                        service.Insert(listFeed, row.row);
                    }

                    newRowWorksheetsNeeded[entry.Title.Text] = true;

                }
                
                ProjectStatusUpdateWorksheet newWorksheet = new ProjectStatusUpdateWorksheet(service, entry);

                if (newWorksheet.IsValid)//.rows.Count > 0)
                {
                    //txtResult.AppendText(newWorksheet.rows.ToString() + Environment.NewLine);
                    worksheets.Add(newWorksheet);
                }
            }
            txtLog.AppendText(Convert.ToString(newRows.Count) + " number of rows were read from emails and need to be added to a worksheet." + Environment.NewLine);

            //some of the emails we parsed do not have worksheets created for them... so lets create them.
            if(newRowWorksheetsNeeded.Values.Contains(false))
            {
                Dictionary<String,bool> newRowWorksheetsNeeded_copy = new Dictionary<String,bool>(newRowWorksheetsNeeded);
                foreach (KeyValuePair<String, bool> keyVal in newRowWorksheetsNeeded_copy)
                {
                    if (keyVal.Value)
                    {
                        //we have already added this worksheet, skip this loop.
                        continue;
                    }
                    String title = keyVal.Key;


                    //create a new worksheet 
                    WorksheetEntry worksheet = new WorksheetEntry();
                    worksheet.Title.Text = title;
                    worksheet.Cols = 21;
                    worksheet.Rows = 1;
                    AtomId id = worksheet.Id;

                    //add to spreadsheet
                    worksheet = service.Insert(wsFeed, worksheet);

                    //create the headers
                    CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                    CellFeed cellFeed = this.service.Query(cellQuery);

                    CellEntry cellEntry = new CellEntry(1, 1, "Send Survey?");
                    cellFeed.Insert(cellEntry);

                    cellEntry = new CellEntry(1, 2, "Date Replied");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 3, "Date project closed");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 4, "Closed project number");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 5, "Client");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 6, "Project name");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 7, "e-mail");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 8, "Name");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 9, "Sales associate (AE");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 10, "SE");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 11, "SPM");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 12, "Lead Dev");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 13, "ADD/Manager");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 14, "Dev Mgr");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 15, "Coffee card");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 16, "Sent?");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 17, "Technical score?");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 18, "Service score?");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 19, "Referral Score ");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 20, "Do they want us to follow up?");
                    cellFeed.Insert(cellEntry);
                    cellEntry = new CellEntry(1, 21, "Action taken/comments");
                    cellFeed.Insert(cellEntry);

                    ProjectStatusUpdateWorksheet newCustomWorksheet = new ProjectStatusUpdateWorksheet(service, worksheet);
                    
                    // Define the URL to request the list feed of the worksheet.
                    AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                    // Fetch the list feed of the worksheet.
                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = service.Query(listQuery);

                    foreach (ProjectStatusUpdateRow row in newRows)
                    {
                        if (row.workSheetTitle == title)
                        {
                            service.Insert(listFeed, row.row);
                        }
                        newCustomWorksheet.rows.Add(row);
                    }

                    //add to custom worksheets object so we can read it later.
                    worksheets.Add(newCustomWorksheet);

                    addedEmailData = true;
                    /**/
                    newRowWorksheetsNeeded[title] = true;
                }

            }

            txtLog.AppendText("Generating email list now..." + Environment.NewLine);
            bool needToSendSurvey = false;
            foreach (ProjectStatusUpdateWorksheet ws in worksheets)
            {
                foreach (ProjectStatusUpdateRow row in ws.rows)
                {
                    if (row.SendSurvey && row.SurveySentDate == null)
                    {
                        String[] name = row.Name.Split(' ');
                        if (name.Length > 1)
                        {
                            String fName = name[0];
                            String lName = name[1];
                            String email = row.email.Trim();
                            if (email != "") 
                            {
                                row.outPutForSending = true;
                                needToSendSurvey = true;
                                txtResult.AppendText(email + ", " + lName + ", " + fName + Environment.NewLine);
                            }
                        }
                    }
                }
            }

            if (needToSendSurvey)
            {
                txtLog.AppendText("Done. Please copy the list of contacts to SurveyMonkey.");
            }
            else
            {
                txtLog.AppendText("No rows were found that need updating... Double check the spreadsheet to ensure this is correct" + Environment.NewLine
                    + "Please contact Russell Dempsey if you feel this is incorrect.");
                txtLog.AppendText(Environment.NewLine + Convert.ToString(worksheets.Count) + " worksheets, and " + Convert.ToString(newRows.Count) + " rows added from emails");
            }
        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            String txtLogOrig = txtLog.Text;
            int count = 0;
            foreach (ProjectStatusUpdateWorksheet worksheet in worksheets)
            {
                foreach (ProjectStatusUpdateRow row in worksheet.rows)
                {
                    if (row.SendSurvey && row.outPutForSending && row.SurveySentDate == null)
                    {
                        foreach (ListEntry.Custom element in row.row.Elements)
                        {
                            if (element.LocalName == "sendsurvey")
                            {
                                element.Value = DateTime.Now.ToString("M/d/yyyy");
                                break;
                            }
                        }
                        row.row.Update();

                        txtLog.Text = txtLogOrig + Environment.NewLine + Convert.ToString(++count) + " rows updated so far...";
                    }
                }
                //worksheet.worksheetEntry.Update();
            }
            txtLog.Text = txtLogOrig + Environment.NewLine + Convert.ToString(++count) + " rows updated.";
        }

        private void txtResult_MouseClick(object sender, MouseEventArgs e)
        {
            txtResult.SelectAll();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //unfocus the main form
            //frmSettings.
            frmSettings settingsForm = new frmSettings();

            settingsForm.ShowDialog();

            txtUserName.Text = Settings.Default.signInEmail;

            //display settings form, make top window and force it to stay so until user exits settings.


        }

    }

    public class ProjectStatusUpdateWorksheet
    {
        public List<ProjectStatusUpdateRow> rows = new List<ProjectStatusUpdateRow>();
        public bool IsValid = false;
        public AtomId Id;
        public WorksheetEntry worksheetEntry;

        public ProjectStatusUpdateWorksheet(SpreadsheetsService service, WorksheetEntry entry)
        {
            this.worksheetEntry = entry;
            this.Id = entry.Id;
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = entry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);

            //loop over each row to see if the survey has been sent.
            foreach (ListEntry row in listFeed.Entries)
            {
                foreach (ListEntry.Custom keyValPair in row.Elements)
                {
                    //verify we're in the correct worksheet by checking the first columns name.
                    if (keyValPair.LocalName != "sendsurvey" || this.IsValid)
                    {
                        break;
                    }
                    this.IsValid = true;
                    break;

                    //we're now dealing only with valid worksheets, so let's read the data!
                    //txtResult.AppendText(keyValPair.LocalName + Environment.NewLine);
                }
                if (this.IsValid)
                {
                    this.addRows(listFeed.Entries);
                    if (this.rows == null)
                    {
                        this.IsValid = false;
                    }
                    return;
                }
            }
        }

        private bool addRows(AtomEntryCollection rows)
        {

            foreach (ListEntry row in rows)
            {
                this.rows.Add(new ProjectStatusUpdateRow(row));
            }
            return true;
        }
    }

    public class ProjectStatusUpdateRow// : IDictionary<String, String>
    {
        //All the data that needs to go from the email to the spreadsheet.
        public bool SendSurvey;
        public DateTime? SurveySentDate;
        public DateTime? DateReplied;
        public DateTime? ProjectClosed;
        public String ProjectNumber;
        public String Client;
        public String ProjectName;
        public String email;
        public String Name;
        public String SalesAssociate;
        public String SE;
        public String SPM;
        public String LeadDev;
        public bool outPutForSending;

        //Data that makes sure each of these objects knows which spreadsheet+worksheet+row it came from
        public String myRow;
        public String myWorksheet;
        public String mySpreadsheet;
        public String workSheetTitle;
        public bool IsValid = false;
        public AtomId Id;
        public ListEntry row;

        //constructor from Google Docs Spreadsheet worksheetEntry row
        public ProjectStatusUpdateRow(ListEntry row)
        {
            this.Id = row.Id;
            this.row = row;
            foreach (ListEntry.Custom keyValPair in row.Elements)
            {
                switch (keyValPair.LocalName)
                {
                    case "sendsurvey":
                        if (keyValPair.Value == "DO NOT SEND")
                        {
                            this.SendSurvey = false;
                            this.SurveySentDate = null;
                        }
                        else
                        {
                            this.SendSurvey = true;
                            try
                            {
                                this.SurveySentDate = Convert.ToDateTime(keyValPair.Value);
                            }
                            catch (FormatException)
                            {
                                this.SurveySentDate = null;
                            }
                        }
                        break;
                    case "datereplied":
                        try
                        {
                            this.DateReplied = Convert.ToDateTime(keyValPair.Value);
                        }
                        catch (FormatException)
                        {
                            this.ProjectClosed = null;
                        }
                        break;
                    case "dateprojectclosed":
                        try
                        {
                            this.ProjectClosed = Convert.ToDateTime(keyValPair.Value);
                        }
                        catch (FormatException)
                        {
                            this.ProjectClosed = null;
                        }
                        break;
                    case "closedprojectnumber":
                        this.ProjectNumber = keyValPair.Value;
                        break;
                    case "client":
                        this.Client = keyValPair.Value;
                        break;
                    case "projectname":
                        this.ProjectName = keyValPair.Value;
                        break;
                    case "e-mail":
                        this.email = keyValPair.Value;
                        break;
                    case "name":
                        this.Name = keyValPair.Value;
                        break;
                    case "salesassociateae":
                        this.SalesAssociate = keyValPair.Value;
                        break;
                    case "se":
                        this.SE = keyValPair.Value;
                        break;
                    case "spm":
                        this.SPM = keyValPair.Value;
                        break;
                    case "leaddev":
                        this.LeadDev = keyValPair.Value;
                        break;
                        /*
                    case "addmanager":
                        this.
                        break;
                    case "devmgr":
                        break;
                    case "coffeecard":
                        break;
                    case "sent":
                        break;
                    case "technicalscore":
                        break;
                    case "servicescore":
                        break;
                    case "referralscore":
                        break;
                    case "dotheywantustofollowup":
                        break;
                    case "actiontakencomments":
                        break;
                        */
                    default:
                        //There shouldn't be anything here.
                        break;
                }
            }
        }

        //constructor from gmail email after parsing the data.
        public ProjectStatusUpdateRow(String emailBody, DateTime date)
        {
            if (this.parseEmailWithRegex(emailBody, date))
            {
                this.IsValid = true;
                this.row = this.newRow();
            }
            /*
            String[] details = new String[] {"Project: ", "Client: ", "Primary Contact: ", "SPM: ", "Lead Developer(s): ", "AE: ", "SE: ", "Date: "};
            String[] result = new String[details.Length];
            if (emailBody.IndexOf("The new status: \"Completed\"") != -1)
            {
                this.IsValid = true;

                int counter = 0;
                foreach(String data in details)
                {
                    int dataIndex = emailBody.IndexOf(data) + data.Length;
                    int endIndex = emailBody.IndexOf(Environment.NewLine,dataIndex);
                    result[counter++] = emailBody.Substring(dataIndex, endIndex-dataIndex);
                }

                //Gillian will be updating the status update emails soon and they will indicate whether a survey should be sent or not.
                //TODO: add parsing to determine whether to send these surveys or not.
                this.SendSurvey = true;

                this.SurveySentDate = null;

                this.ProjectName = result[0];
                //String projNum = result[0].Split('(')[1];
                this.ProjectNumber = result[0].Split('(')[1].Replace(")", "");

                this.Client = result[1];

                //String tempName = result[2].Split('(')[0].Trim();
                this.Name = result[2].Split('(')[0].Trim();
                //String tempEmail = result[2].Split('(')[1];
                this.email = result[2].Split('(')[1].Replace(")", "");

                this.SPM = result[3];

                this.LeadDev = result[4];

                this.SalesAssociate = result[5];

                this.SE = result[6];

                this.ProjectClosed = DateTime.Parse(result[7].Substring(0,result[7].IndexOf(" at ")));
                this.ProjectClosed = date;

                this.workSheetTitle = ((DateTime)this.ProjectClosed).ToString("MMMM yyyy");

                this.row = this.newRow();
            }
             * */
        }

        public bool parseEmailWithRegex(String emailBody, DateTime date)
        {
            String pattern = "The new status: \"Completed\"";
            if (Regex.IsMatch(emailBody, pattern))
            {
                pattern = "Project: (.*)";
                this.ProjectName = Regex.Match(emailBody, pattern).Groups[1].Value;
                
                pattern = "\\((.*)\\)";
                this.ProjectNumber = Regex.Match(this.ProjectName, pattern).Groups[1].Value;

                pattern = "Client: (.*)";
                this.Client = Regex.Match(emailBody, pattern).Groups[1].Value;
                

                pattern = "Primary Contact: (.*)";
                String nameWemail = Regex.Match(emailBody, pattern, RegexOptions.Multiline).Groups[1].Value;

                var nameGroups = Regex.Match(nameWemail, "(.+) \\(|(.*)").Groups;
                this.Name = (nameGroups[1].Value.Trim() == "") ? nameGroups[2].Value.Trim() : nameGroups[1].Value.Trim();

                //pattern = "Primary Contact: [^(]*\\(([^\\)]*)\\)";
                this.email = Regex.Match(nameWemail, "\\((.*)\\)").Groups[1].Value;
                       

                pattern = "SPM: ([^\\n]*)";
                this.SPM = Regex.Match(emailBody, pattern).Groups[1].Value;

                pattern = "Lead Developer\\(s\\): ([^\\r\\n]*)";
                this.LeadDev = Regex.Match(emailBody, pattern).Groups[1].Value;

                pattern = "AE: ([^\\r\\n]*)";
                this.SalesAssociate = Regex.Match(emailBody, pattern).Groups[1].Value;

                pattern = "SE: ([^\\r\\n]*)";
                this.SE = Regex.Match(emailBody, pattern).Groups[1].Value;


                this.ProjectClosed = date;

                this.workSheetTitle = date.ToString("MMMM yyyy");

                this.SendSurvey = true;

                this.SurveySentDate = null;

                return true;
            }

            return false;
        }

        public ProjectStatusUpdateRow()
        {
            this.row = this.getHeaderRow();
        }

        public ListEntry newRow()
        {
            // Create a local representation of the new row.
            ListEntry row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = "sendsurvey", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "datereplied", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "dateprojectclosed", Value = ((DateTime)this.ProjectClosed).ToString("M/d/yyyy") });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "closedprojectnumber", Value = this.ProjectNumber });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "client", Value = this.Client });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "projectname", Value = this.ProjectName });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "e-mail", Value = this.email });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "name", Value = this.Name });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "salesassociateae", Value = this.SalesAssociate });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "se", Value = this.SE });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "spm", Value = this.SPM });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "leaddev", Value = this.LeadDev });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "addmanager", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "devmgr", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "coffeecard", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "sent", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "technicalscore", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "servicescore", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "referralscore", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "dotheywantustofollowup", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "actiontakencomments", Value = "" });

            return row;
        }

        public ListEntry getHeaderRow()
        {
            // Create a local representation of the new row.
            ListEntry row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = "sendsurvey", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "datereplied", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "dateprojectclosed", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "closedprojectnumber", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "client", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "projectname", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "e-mail", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "name", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "salesassociateae", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "se", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "spm", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "leaddev", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "addmanager", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "devmgr", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "coffeecard", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "sent", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "technicalscore", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "servicescore", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "referralscore", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "dotheywantustofollowup", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "actiontakencomments", Value = "" });

            return row;
        }
    }
}
