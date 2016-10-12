###
MVC
###

.. class:: CorsDelegatingHandler

    The ``DelegatingHandler`` that adds CORS headers to every response. Client should send ``Origin`` header. In response ``Access-Control-Allow-Origin`` will be sent.

.. class:: DifferentFolderRazorViewEngine

    Allows to override default folder (``Views``) for templates. Example of registration:

    .. code-block:: c#

        protected void Application_Start(object sender, EventArgs e)
        {
            // ..
            // Register our customer view engine to control T2 and TBag views and over ridding
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new DifferentFolderRazorViewEngine("AnotherViews"));
        }
