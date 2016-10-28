/*
 * Fake task service that needs only for development.
 */

define(['jquery'], function ($) {
    'use strict';

    return {
        get: function() {
            return new Promise(function (resolve, reject) {
                resolve([{
                    Id: 1,
                    Text: 'Prepare presentation to frontend meeting',
                    ProjectId: 1,
                    isEdit: false
                }, {
                    Id: 2,
                    Text: 'Sort CRM tasks',
                    ProjectId: 2,
                    isDone: false,
                    isEdit: true
                }]);
            });
        },

        getProjects: function() {
            return new Promise(function (resolve, reject) {
                resolve([{
                    Id: 1,
                    Name: 'Work (test)',
                    Color: null
                }, {
                    Id: 2,
                    Name: 'Home (test)',
                    Color: null
                }]);
            });
        },

        update: function(task) {
            return new Promise(function(resolve) {
                resolve();
            });
        },

        remove: function(task) {
            return new Promise(function(resolve) {
                resolve();
            });
        },

        check: function(task) {
            return new Promise(function(resolve) {
                resolve();
            });
        }
    }
});
