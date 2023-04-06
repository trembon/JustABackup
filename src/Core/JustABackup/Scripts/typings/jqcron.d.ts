declare namespace JQCron {
    interface JQCronInstance {
        disable(): void;
    }
}

interface JQuery {
    jqCron(params: any): void;

    jqCronGetInstance(): JQCron.JQCronInstance;
}