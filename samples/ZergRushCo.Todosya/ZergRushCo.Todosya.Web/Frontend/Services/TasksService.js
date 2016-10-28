/*
 * Tasks handlers.
 */

define(['App', 'jquery', 'knockout'], function (App, $, ko) {
    'use strict';

    return {
        /*
         * Returns all users tasks, returns promise.
         */
        get: function () {
            return $.ajax({
                url: '/json/tasks'
            });
        },

        /*
         * Returns all users projects, returns promise.
         */
        getProjects: function () {
            return $.ajax({
                url: '/json/projects'
            });
        },

        /*
         * Create task request, returns promise.
         */
        create: function (task) {
            return $.ajax({
                url: '/json/tasks',
                type: 'post',
                contentType: App.contentTypeJson,
                data: JSON.stringify(ko.toJS(task))
            });
        },

        /*
         * Update task request, returns promise.
         */
        update: function (task) {
            return $.ajax({
                url: '/json/tasks',
                type: 'put',
                contentType: App.contentTypeJson,
                data: JSON.stringify(ko.toJS(task))
            });
        },

        /*
         * Remove task, returns promise.
         */
        remove: function (id) {
            return $.ajax({
                url: '/json/tasks/' + id,
                type: 'delete'
            });
        },

        /*
         * Check or uncheck task, returns promise.
         */
        check: function (id, isDone) {
            return $.ajax({
                url: '/json/tasks/' + id + '/check',
                type: 'post',
                data: JSON.stringify({
                    taskId: id,
                    isDone: isDone
                }),
                contentType: App.contentTypeJson
            });
        }
    }
});
