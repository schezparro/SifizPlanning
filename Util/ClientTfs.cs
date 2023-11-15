using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Work;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Net;

namespace SifizPlanning.Util
{
    public class ClientTfs
    {        
        public static void SampleREST()
        {
            /*
            string teamProjectName = "ScrumSFZ";
            // Create a connection object, which we will use to get httpclient objects.  This is more robust
            // then newing up httpclient objects directly.  Be sure to send in the full collection uri.
            // For example:  http://myserver:8080/tfs/defaultcollection
            // We are using default VssCredentials which uses NTLM against a Team Foundation Server.  See additional provided
            // examples for creating credentials for other types of authentication.

            TfsTeamProjectCollection collection = new TfsTeamProjectCollection(new Uri("http://TFSServer:8080/tfs/DefaultCollection"));
            WorkItemServer svc = collection.GetService(typeof(WorkItemServer)) as WorkItemServer;


            string collectionUri = "http://200.125.0.178:8080/tfs/Financial2013";
            VssConnection connection = new VssConnection(new Uri(collectionUri), new VssCredentials());
            
            // Create instance of WorkItemTrackingHttpClient using VssConnection
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            // Get 2 levels of query hierarchy items
            List<QueryHierarchyItem> queryHierarchyItems = witClient.GetQueriesAsync(teamProjectName).Result;
            //List<QueryHierarchyItem> queryHierarchyItems = witClient.GetQueriesAsync(teamProjectName, depth: 2).Result;

            // Search for 'My Queries' folder
            QueryHierarchyItem myQueriesFolder = queryHierarchyItems.FirstOrDefault(qhi => qhi.Name.Equals("My Queries"));
            if (myQueriesFolder != null)
            {
                string queryName = "REST Sample";

                // See if our 'REST Sample' query already exists under 'My Queries' folder.
                QueryHierarchyItem newBugsQuery = null;
                if (myQueriesFolder.Children != null)
                {
                    newBugsQuery = myQueriesFolder.Children.FirstOrDefault(qhi => qhi.Name.Equals(queryName));
                }
                if (newBugsQuery == null)
                {
                    // if the 'REST Sample' query does not exist, create it.
                    newBugsQuery = new QueryHierarchyItem()
                    {
                        Name = queryName,
                        Wiql = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.WorkItemType] = 'Bug' AND [System.State] = 'New'",
                        IsFolder = false
                    };
                    newBugsQuery = witClient.CreateQueryAsync(newBugsQuery, teamProjectName, myQueriesFolder.Name).Result;
                }

                // run the 'REST Sample' query
                WorkItemQueryResult result = witClient.QueryByIdAsync(newBugsQuery.Id).Result;

                if (result.WorkItems.Any())
                {
                    int skip = 0;
                    const int batchSize = 100;
                    IEnumerable<WorkItemReference> workItemRefs;
                    do
                    {
                        workItemRefs = result.WorkItems.Skip(skip).Take(batchSize);
                        if (workItemRefs.Any())
                        {
                            // get details for each work item in the batch
                            List<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem> workItems = witClient.GetWorkItemsAsync(workItemRefs.Select(wir => wir.Id)).Result;
                            foreach (Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem in workItems)
                            {
                                // write work item to console
                                Console.WriteLine("{0} {1}", workItem.Id, workItem.Fields["System.Title"]);
                            }
                        }
                        skip += batchSize;
                    }
                    while (workItemRefs.Count() == batchSize);
                }
                else
                {
                    Console.WriteLine("No work items were returned from query.");
                }
            }*/
        }
        
        public static int AdicionarTrabajo( string urlTfs, string nombreColeccion, string nombreProjecto, string tipoTrabajo, string usuario, string titulo, string detalle, string numeroTicket, int tiempoTarea )
        {
            Uri collectionUri = new Uri(urlTfs + "/" + nombreColeccion);
            NetworkCredential cred = new NetworkCredential("evelin.torres", "vbnvb5435", "");
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(collectionUri, cred);
            tpc.EnsureAuthenticated();
            WorkItemStore workItemStore = tpc.GetService<WorkItemStore>();
            Project teamProject = workItemStore.Projects[nombreProjecto];
            WorkItemType workItemType = teamProject.WorkItemTypes[tipoTrabajo];

            if (titulo.Length > 250)
            {
                titulo = titulo.Substring(0, 250) + "...";
            }   
            // Creando el workItem
            WorkItem workItem = new WorkItem(workItemType)
            {
                Title = titulo,
                Description = detalle
            };

            workItem.Fields["Asignado a"].Value = usuario;
            workItem.Fields["Identificador"].Value = numeroTicket;
            workItem.Fields["TipoTrabajo"].Value = "Ticket";
            workItem.Fields["Actividad"].Value = "Development";
            workItem.Fields["Trabajo restante"].Value = tiempoTarea;

            //if (nombreColeccion.IndexOf("2010") != -1)
            //{
            //    workItem.Fields["Asignado a"].Value = usuario;
            //    workItem.Fields["Identificador"].Value = numeroTicket;
            //    workItem.Fields["TipoTrabajo"].Value = "Ticket";
            //    workItem.Fields["Activity"].Value = "Development";
            //    workItem.Fields["Remaining Work"].Value = tiempoTarea;
            //}
            //else
            //{
            //    workItem.Fields["Asignado a"].Value = usuario;
            //    workItem.Fields["Identificador"].Value = numeroTicket;
            //    workItem.Fields["TipoTrabajo"].Value = "Ticket";
            //    workItem.Fields["Actividad"].Value = "Development";
            //    workItem.Fields["Trabajo restante"].Value = tiempoTarea;
            //}

            var result = workItem.Validate();

            // Save the new workItem. 
            workItem.Save();
            int id = workItem.Id;

            return id;
        }

        public void UpdateTFSWorkItemValue(string tfsServerUrl, string fieldToUpdate, string valueToUpdateTo, int workItemID)
        {
            // Connect to the TFS Server
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsServerUrl));
            // Connect to the store of work items.
            WorkItemStore _store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
            // Grab the work item we want to update
            WorkItem workItem = _store.GetWorkItem(workItemID);
            // Open it up for editing.  (Sometimes PartialOpen() works too and takes less time.)
            workItem.Open();
            // Update the field.
            workItem.Fields[fieldToUpdate].Value = valueToUpdateTo;
            // Save your changes.  If there is a constraint on the field and your value does not 
            // meet it then this save will fail. (Throw an exception.)  I leave that to you to
            // deal with as you see fit.
            workItem.Save();
        }
    }
}