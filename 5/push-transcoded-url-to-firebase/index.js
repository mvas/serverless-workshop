'use strict';

/**
 * Created by Peter Sbarski
 * Updated by Mike Chambers
 * Updated by Julian Pittas
 * Last Updated: 28/02/2018
 *
 * Required Env Vars:
 * SERVICE_ACCOUNT
 * DATABASE_URL
 * S3_TRANSCODED_BUCKET_URL : https://s3.amazonaws.com/YOUR_TRANSCODED_BUCKET_NAME_HERE
 */

const AWS = require('aws-sdk');
const firebase = require('firebase');

// save the URL to firebase
firebase.initializeApp({
    serviceAccount: process.env.SERVICE_ACCOUNT,
    databaseURL: process.env.DATABASE_URL
});

const handler = (event, context, callback) => {

    context.callbackWaitsForEmptyEventLoop = false;

    const key = event.Records[0].s3.object.key;
    const bucket = event.Records[0].s3.bucket.name;

    const videoUrl = process.env.S3 + '/' + key;

    // construct S3 URL based on bucket and key
    // the input file may have spaces so replace them with '+'
    const sourceKey = decodeURIComponent(key.replace(/\+/g, ' '));

    // get the unique video key (the folder name)
    const uniqueVideoKey = sourceKey.split('/')[0];

    const database = firebase.database().ref();

    // update the unique entry for this video in firebase
    return database.child('videos').child(uniqueVideoKey).set({
        transcoding: false,
        source: videoUrl
    })
    .then(() => {
        callback(null, `Added URL ${videoUrl}`);
    })
    .catch((err) => {
        callback(err);
    });
};

module.exports = {
    handler
};