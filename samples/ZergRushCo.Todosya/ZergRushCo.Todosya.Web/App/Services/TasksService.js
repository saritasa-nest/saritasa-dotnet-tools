define(['App', 'jquery', 'knockout'], function (App, $, ko) {
    'use strict';

    return {
        get: function () {
            return $.ajax({
                url: '/json/tasks'
            });
        },

        getProjects: function () {
            return $.ajax({
                url: '/json/projects'
            });
        },

        create: function (task) {
            return $.ajax({
                url: '/json/tasks',
                type: 'post',
                contentType: App.contentTypeJson,
                data: JSON.stringify(ko.toJS(task))
            });
        },

        update: function (task) {
            return $.ajax({
                url: '/json/tasks',
                type: 'put',
                contentType: App.contentTypeJson,
                data: JSON.stringify(ko.toJS(task))
            });
        },

        remove: function (id) {
            return $.ajax({
                url: '/json/tasks/' + id,
                type: 'delete'
            });
        },

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
