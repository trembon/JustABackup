namespace InputTemplates {

    interface DropDownValues {

        id: string;

        name: string;

    }

    export function generateInput(field: DynamicFormField, providerSection: number): JQuery {
        let $wrapper = $(`<div class="form-group"></div>`);

        switch (field.type) {
            case 'hidden':
                generateHiddenInput(field, $wrapper, providerSection);
                break;

            case 'string':
                generateStringInput(field, $wrapper, providerSection);
                break;

            case 'dropdown':
                generateDropdownInput(field, $wrapper, providerSection);
                break;

            case 'multi-select':
                generateMultiSelectInput(field, $wrapper, providerSection);
                break;

            case 'cron':
                generateCronInput(field, $wrapper, providerSection);
                break;
        }

        return $wrapper;
    }

    function generateHiddenInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): void {
        let fieldName = generateFieldName(field, providerSection);
        let value = field.value ? field.value : "";

        console.log('generateHiddenInput', fieldName, field);
        $wrapper.removeClass("form-group").addClass("hidden");
        $wrapper.append(`<input type="hidden" name="${fieldName}" value="${value}" />`);
    }

    function generateStringInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): void {
        let fieldName = generateFieldName(field, providerSection);
        let value = field.value ? field.value : "";

        console.log('generateStringInput', fieldName, field);
        appendLabel($wrapper, field.name, fieldName);
        $wrapper.append(`<input class="form-control boxed" type="text" id="${fieldName}" name="${fieldName}" value="${value}">`);
    }

    function generateDropdownInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): void {
        let fieldName = generateFieldName(field, providerSection);

        console.log('generateDropdownInput', fieldName, field);
        appendLabel($wrapper, field.name, fieldName);

        let $list = $(`<select class="form-control boxed" id="${fieldName}" name="${fieldName}"></select>`);
        $list.appendTo($wrapper);

        if (field.dataSource) {
            $.getJSON(field.dataSource, data => {
                (<DropDownValues[]>data).forEach(val => {
                    $list.append(`<option value="${val.id}">${val.name}</option>`);
                });

                $list.val(field.value);
            });
        }
    }

    function generateMultiSelectInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): void {
        let fieldName = generateFieldName(field, providerSection);

        console.log('generateMultiSelectInput', fieldName, field);
        appendLabel($wrapper, field.name, fieldName);

        let $list = $(`<select class="form-control boxed" multiple id="${fieldName}" name="${fieldName}"></select>`);
        $list.appendTo($wrapper);

        if (field.dataSource) {
            $.getJSON(field.dataSource, data => {
                (<DropDownValues[]>data).forEach(val => {
                    $list.append(`<option value="${val.id}">${val.name}</option>`);
                });

                $list.val(field.value);
            });
        }
    }

    function generateCronInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): void {
        let fieldName = generateFieldName(field, providerSection);
        let value = field.value ? field.value : "";

        console.log('generateCronInput', fieldName, field);
        appendLabel($wrapper, field.name, fieldName);
        $wrapper.append(`<div class="form-control boxed"><input class="cron" type="hidden" id="${fieldName}" name="${fieldName}" /></div>`);
    }

    function appendLabel($wrapper: JQuery, label: string, fieldName: string): void {
        $wrapper.append(`<label class="control-label" for="${fieldName}">${label}</label>`);
    }

    function generateFieldName(field: DynamicFormField, providerSection: number): string {
        if (providerSection <= 1) {
            return field.id;
        }

        return `Providers[${providerSection - 2}].${field.id}`; 
    }
}