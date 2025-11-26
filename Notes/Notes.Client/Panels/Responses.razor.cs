using Microsoft.AspNetCore.Components;
using Notes.Protos;
using Syncfusion.Blazor.Grids;

namespace Notes.Client.Panels
{
    public partial class Responses
    {
        /// <summary>
        /// List of response headers
        /// </summary>
        /// <value>The headers.</value>
        [Parameter] public List<GNoteHeader> Headers { get; set; }

        /// <summary>
        /// Show content for responses
        /// </summary>
        /// <value><c>true</c> if [show content r]; otherwise, <c>false</c>.</value>
        [Parameter] public bool ShowContentR { get; set; }

        /// <summary>
        /// Expand all rows
        /// </summary>
        /// <value><c>true</c> if [expand all r]; otherwise, <c>false</c>.</value>
        [Parameter] public bool ExpandAllR { get; set; }

        //[Parameter] public NoteIndex Parent {get; set;}

        //public bool ShowContent { get; set; }
        //public bool ExpandAll { get; set; }


        /// <summary>
        /// Gets or sets the sf grid2.
        /// </summary>
        /// <value>The sf grid2.</value>
        protected SfGrid<GNoteHeader> sfGrid2 { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject] NavigationManager Navigation { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Responses"/> class.
        /// </summary>
        public Responses()
        {
        }

        /// <summary>
        /// Copy parameter to local copy
        /// </summary>
        //protected override async Task OnParametersSetAsync()
        //{
        //}

        public void DataBoundHandler()
        {
            // Expand if appropriate
            if (ExpandAllR)
            {
                sfGrid2.ExpandAllDetailRowAsync().GetAwaiter();
            }
        }

        //System.Timers.Timer timer2;

        //protected void ActionCompleteHandler(ActionEventArgs<GNoteHeader> action)
        //{
        //    if (action.RequestType == Syncfusion.Blazor.Grids.Action.Filtering)
        //    {
        //        timer2 = new System.Timers.Timer(1000);
        //        timer2.Elapsed += TimerTick2;
        //        timer2.Enabled = true;
        //    }
        //}

        //protected void TimerTick2(Object source, ElapsedEventArgs e)
        //{
        //    timer2.Elapsed -= TimerTick2;
        //    timer2.Stop();
        //    timer2.Enabled = false;

        //    this.StateHasChanged();
        //}

        /// <summary>
        /// GO show the note
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void DisplayIt(RowSelectEventArgs<GNoteHeader> args)
        {
            Navigation.NavigateTo("notedisplay/" + args.Data.Id);
        }
    }
}