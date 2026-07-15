{{- define "vertex.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- define "vertex.fullname" -}}
{{- if .Values.fullnameOverride }}{{ .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}{{ else }}{{ include "vertex.name" . }}{{ end }}
{{- end }}
{{- define "vertex.labels" -}}
app.kubernetes.io/name: {{ include "vertex.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
{{- define "vertex.selectorLabels" -}}
app.kubernetes.io/name: {{ include "vertex.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

