﻿/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes zones
    /// </summary>
    class DeleteZones : DeleteCommandBase<Zone>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteZones";

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        #endregion

        #region DeleteCommandBase Protected Methods

        /// <summary>
        /// Deletes zones
        /// </summary>
        protected override void _Delete(IList<Zone> selectedObjects)
        {
            var deletionChecker = _Application.Project.DeletionCheckingService;
            var ids = selectedObjects.Select(_ => _.Id).ToList();

            var zone = deletionChecker.QueryDefaultRouteZones(selectedObjects).FirstOrDefault();
            if (zone != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_DEFAULT_ROUTE_KEY, zone);
                _Application.Messenger.AddError(message);

                return;
            }

            zone = deletionChecker.QueryFutureRouteZones(selectedObjects, DateTime.Now)
                .FirstOrDefault();
            if (zone != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_FUTURE_ROUTE_KEY, zone);
                _Application.Messenger.AddError(message);

                return;
            }

            foreach (Zone zn in selectedObjects)
                App.Current.Project.Zones.Remove(zn);

            App.Current.Project.Save();
        }

        #endregion DeleteCommandBase Protected Methods

        #region DeleteCommandBase Protected Properties

        protected override ISupportDataObjectEditing ParentPage
        {
            get 
            {
                if (_parentPage == null)
                {
                    ZonesPage page = (ZonesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.ZonesPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DeleteCommandBase Protected Properties

        #region Private Methods

        private void _FilterZones(IDataObjectCollection<Route> routes, IList filterObjects,
                                     ref Collection<ESRI.ArcLogistics.Data.DataObject> filtredObjects)
        {
            for (int index = 0; index < routes.Count; ++index)
                _FilterObjects(routes[index].Zones, filterObjects, ref filtredObjects);
        }

        #endregion Private Methods

        #region private constants
        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// default route.
        /// </summary>
        private const string ASSIGNED_TO_DEFAULT_ROUTE_KEY = "ZoneAssignedToDefaultRoute";

        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// route planned for current date or any date after it.
        /// </summary>
        private const string ASSIGNED_TO_FUTURE_ROUTE_KEY = "ZoneAssignedToFutureRoute";
        #endregion

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}
